using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace TrumguSignalR.MongoDB
{
    public class MongoDbHelper
    {
        private readonly string _connectionString;
        private readonly string _databaseName;
        private IMongoDatabase _database;
        private readonly bool _autoCreateDb;
        private readonly bool _autoCreateCollection;

        static MongoDbHelper()
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;
        }

        public MongoDbHelper(string mongoConnStr, string dbName, bool autoCreateDb = false, bool autoCreateCollection = false)
        {
            _connectionString = mongoConnStr;
            _databaseName = dbName;
            _autoCreateDb = autoCreateDb;
            _autoCreateCollection = autoCreateCollection;
        }

        #region 私有方法

        private MongoClient CreateMongoClient()
        {
            return new MongoClient(_connectionString);
        }


        private IMongoDatabase GetMongoDatabase()
        {
            if (_database != null)
            {
                return _database;
            }
            var client = CreateMongoClient();
            if (!DatabaseExists(client, _databaseName) && !_autoCreateDb)
            {
                throw new KeyNotFoundException("此MongoDB名称不存在：" + _databaseName);
            }

            _database = CreateMongoClient().GetDatabase(_databaseName);

            return _database;
        }

        private bool DatabaseExists(MongoClient client, string dbName)
        {
            try
            {
                var dbNames = client.ListDatabases().ToList().Select(db => db.GetValue("name").AsString);
                return dbNames.Contains(dbName);
            }
            catch //如果连接的账号不能枚举出所有DB会报错，则默认为true
            {
                return true;
            }

        }

        private bool CollectionExists(IMongoDatabase database, string collectionName)
        {
            var options = new ListCollectionsOptions
            {
                Filter = Builders<BsonDocument>.Filter.Eq("name", collectionName)
            };

            return database.ListCollections(options).ToEnumerable().Any();
        }


        private IMongoCollection<TDoc> GetMongoCollection<TDoc>(string name, MongoCollectionSettings settings = null)
        {
            var mongoDatabase = GetMongoDatabase();

            if (!CollectionExists(mongoDatabase, name) && !_autoCreateCollection)
            {
                throw new KeyNotFoundException("此Collection名称不存在：" + name);
            }

            return mongoDatabase.GetCollection<TDoc>(name, settings);
        }

        private List<UpdateDefinition<TDoc>> BuildUpdateDefinition<TDoc>(object doc, string parent)
        {
            var updateList = new List<UpdateDefinition<TDoc>>();
            foreach (var property in typeof(TDoc).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var key = parent == null ? property.Name : $"{parent}.{property.Name}";
                //非空的复杂类型
                if ((property.PropertyType.IsClass || property.PropertyType.IsInterface) && property.PropertyType != typeof(string) && property.GetValue(doc) != null)
                {
                    if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        #region 集合类型
                        var i = 0;
                        var subObj = property.GetValue(doc);
                        foreach (var item in (IList) subObj)
                        {
                            if (item.GetType().IsClass || item.GetType().IsInterface)
                            {
                                updateList.AddRange(BuildUpdateDefinition<TDoc>(doc, $"{key}.{i}"));
                            }
                            else
                            {
                                updateList.Add(Builders<TDoc>.Update.Set($"{key}.{i}", item));
                            }
                            i++;
                        }
                        #endregion
                    }
                    else
                    {
                        #region 实体类型
                        //复杂类型，导航属性，类对象和集合对象 
                        var subObj = property.GetValue(doc);
                        foreach (var sub in property.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                        {
                            updateList.Add(Builders<TDoc>.Update.Set($"{key}.{sub.Name}", sub.GetValue(subObj)));
                        }
                        #endregion
                    }
                }
                else //简单类型
                {
                    updateList.Add(Builders<TDoc>.Update.Set(key, property.GetValue(doc)));
                }
            }

            return updateList;
        }


        private void CreateIndex<TDoc>(IMongoCollection<TDoc> col, string[] indexFields, CreateIndexOptions options = null)
        {
            if (indexFields == null)
            {
                return;
            }
            var indexKeys = Builders<TDoc>.IndexKeys;
            IndexKeysDefinition<TDoc> keys = null;
            if (indexFields.Length > 0)
            {
                keys = indexKeys.Descending(indexFields[0]);
            }
            for (var i = 1; i < indexFields.Length; i++)
            {
                var strIndex = indexFields[i];
                keys = keys.Descending(strIndex);
            }

            if (keys != null)
            {
#pragma warning disable 618
                col?.Indexes?.CreateOne(keys,options);
#pragma warning restore 618

            }

        }

        #endregion

        public void CreateCollectionIndex<TDoc>(string collectionName, string[] indexFields, CreateIndexOptions options = null)
        {
            CreateIndex(GetMongoCollection<TDoc>(collectionName), indexFields, options);
        }

        public void CreateCollection<TDoc>(string[] indexFields = null, CreateIndexOptions options = null)
        {
            string collectionName = typeof(TDoc).Name;
            CreateCollection<TDoc>(collectionName, indexFields, options);
        }

        public void CreateCollection<TDoc>(string collectionName, string[] indexFields = null, CreateIndexOptions options = null)
        {
            var mongoDatabase = GetMongoDatabase();
            mongoDatabase.CreateCollection(collectionName);
            CreateIndex(GetMongoCollection<TDoc>(collectionName), indexFields, options);
        }


        public List<TDoc> Find<TDoc>(Expression<Func<TDoc, bool>> filter, FindOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            return Find(collectionName, filter, options);
        }

        public List<TDoc> Find<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, FindOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            return collection.Find(filter, options).ToList();
        }


        public List<TDoc> FindByPage<TDoc, TResult>(Expression<Func<TDoc, bool>> filter, Expression<Func<TDoc, TResult>> keySelector, int pageIndex, int pageSize, string sort, out int rsCount)
        {
            var collectionName = typeof(TDoc).Name;
            return FindByPage(collectionName, filter, keySelector, pageIndex, pageSize, sort, out rsCount);
        }

        public List<TDoc> FindByPage<TDoc, TResult>(string collectionName, Expression<Func<TDoc, bool>> filter, Expression<Func<TDoc, TResult>> keySelector, int pageIndex, int pageSize, string sort, out int rsCount)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            rsCount = collection.AsQueryable().Where(filter).Count();

            var pageCount = rsCount / pageSize + ((rsCount % pageSize) > 0 ? 1 : 0);
            if (pageIndex > pageCount) pageIndex = pageCount;
            if (pageIndex <= 0) pageIndex = 1;

            var sortDefList = new List<SortDefinition<TDoc>>();
            if (sort != null)
            {
                var sortList = sort.Split(',');
                foreach (var item in sortList)
                {
                    var sl = Regex.Replace(item.Trim(), @"\s+", " ").Split(' ');
                    if (sl.Length == 1 || (sl.Length >= 2 && sl[1].ToLower() == "asc"))
                    {
                        sortDefList.Add(Builders<TDoc>.Sort.Ascending(sl[0]));
                    }
                    else if (sl.Length >= 2 && sl[1].ToLower() == "desc")
                    {
                        sortDefList.Add(Builders<TDoc>.Sort.Descending(sl[0]));
                    }
                }
            }
            var sortDef = Builders<TDoc>.Sort.Combine(sortDefList);

            var result = collection.Find(filter).Sort(sortDef).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync().Result;
            return result.ToList();
        }

        public void Insert<TDoc>(TDoc doc, InsertOneOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            Insert(collectionName, doc, options);
        }

        public void Insert<TDoc>(string collectionName, TDoc doc, InsertOneOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            collection.InsertOne(doc, options);
        }


        public void InsertMany<TDoc>(IEnumerable<TDoc> docs, InsertManyOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            InsertMany(collectionName, docs, options);
        }

        public void InsertMany<TDoc>(string collectionName, IEnumerable<TDoc> docs, InsertManyOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            collection.InsertMany(docs, options);
        }

        public void Update<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            var collection = GetMongoCollection<TDoc>(collectionName);
            var updateList = BuildUpdateDefinition<TDoc>(doc, null);
            collection.UpdateOne(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }

        public void Update<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            var updateList = BuildUpdateDefinition<TDoc>(doc, null);
            collection.UpdateOne(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }


        public void Update<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateDefinition<TDoc> updateFields, UpdateOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            Update(collectionName, doc, filter, updateFields, options);
        }

        public void Update<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateDefinition<TDoc> updateFields, UpdateOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            collection.UpdateOne(filter, updateFields, options);
        }


        public void UpdateMany<TDoc>(TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            UpdateMany(collectionName, doc, filter, options);
        }


        public void UpdateMany<TDoc>(string collectionName, TDoc doc, Expression<Func<TDoc, bool>> filter, UpdateOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            var updateList = BuildUpdateDefinition<TDoc>(doc, null);
            collection.UpdateMany(filter, Builders<TDoc>.Update.Combine(updateList), options);
        }


        public void Delete<TDoc>(Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            Delete(collectionName, filter, options);
        }

        public void Delete<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            collection.DeleteOne(filter, options);
        }


        public void DeleteMany<TDoc>(Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            var collectionName = typeof(TDoc).Name;
            DeleteMany(collectionName, filter, options);
        }


        public void DeleteMany<TDoc>(string collectionName, Expression<Func<TDoc, bool>> filter, DeleteOptions options = null)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            collection.DeleteMany(filter, options);
        }

        public void ClearCollection<TDoc>(string collectionName)
        {
            var collection = GetMongoCollection<TDoc>(collectionName);
            var inddexs = collection.Indexes.List();
            var docIndexs = new List<IEnumerable<BsonDocument>>();
            while (inddexs.MoveNext())
            {
                docIndexs.Add(inddexs.Current);
            }
            var mongoDatabase = GetMongoDatabase();
            mongoDatabase.DropCollection(collectionName);

            if (!CollectionExists(mongoDatabase, collectionName))
            {
                CreateCollection<TDoc>(collectionName);
            }

            if (docIndexs.Count > 0)
            {
                collection = mongoDatabase.GetCollection<TDoc>(collectionName);
                foreach (var index in docIndexs)
                {
                    foreach (IndexKeysDefinition<TDoc> indexItem in index)
                    {
                        try
                        {
#pragma warning disable 618
                            collection?.Indexes?.CreateOne(indexItem);
#pragma warning restore 618
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

        }
        public List<T> Get<T>(string collectionName, Expression<Func<T, bool>> condition, int skip, int limit, string sort)
        {
            return Get(collectionName, new List<Expression<Func<T, bool>>> { condition }, skip, limit, sort);
        }
        public List<TDoc> Get<TDoc>(string collectionName, List<Expression<Func<TDoc, bool>>> conditions, int skip, int limit, string sort)
        {
            if (conditions == null || conditions.Count == 0)
            {
                conditions = new List<Expression<Func<TDoc, bool>>> { x => true };
            }
            var builder = Builders<TDoc>.Filter;
            var filter = builder.And(conditions.Select(x => builder.Where(x)));

            var ret = new List<TDoc>();
            try
            {
                var sortDefList = new List<SortDefinition<TDoc>>();
                if (sort != null)
                {
                    var sortList = sort.Split(',');
                    for (var i = 0; i < sortList.Length; i++)
                    {
                        var sl = Regex.Replace(sortList[i].Trim(), @"\s+", " ").Split(' ');
                        if (sl.Length == 1 || (sl.Length >= 2 && sl[1].ToLower() == "asc"))
                        {
                            sortDefList.Add(Builders<TDoc>.Sort.Ascending(sl[0]));
                        }
                        else if (sl.Length >= 2 && sl[1].ToLower() == "desc")
                        {
                            sortDefList.Add(Builders<TDoc>.Sort.Descending(sl[0]));
                        }
                    }
                }
                var mongoDatabase = GetMongoDatabase();
                mongoDatabase.GetCollection<TDoc>(collectionName);

                var sortDef = Builders<TDoc>.Sort.Combine(sortDefList);
                ret = Find(collectionName, filter).Sort(sortDef).Skip(skip).Limit(limit).ToListAsync().Result;
            }
            catch (Exception)
            {
                //Logger.Error("Get is error", GetType(), ex);
            }
            return ret;
        }
        public IFindFluent<T, T> Find<T>(string collectionName, FilterDefinition<T> filter, FindOptions options = null)
        {
            var mongoDatabase = GetMongoDatabase();
            var collection = mongoDatabase.GetCollection<T>(collectionName);

            return collection.Find(filter, options);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlSugar;
using TrumguSignalR.MySql.IDAL;

namespace TrumguSignalR.MySql.DAL
{
    public class BaseDal<T>: IBaseDal<T> where T:class,new()
    {
        public int Add(T entity)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Insertable(entity).ExecuteCommand();
                return result;
            }
        }

        public int AddReturnIdentity(T entity)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Insertable(entity).ExecuteReturnIdentity();
                return result;
            }
        }

        public T AddReturnEntity(T entity)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Insertable(entity).ExecuteReturnEntity();
                return result;
            }
        }

        public bool AddReturnBool(T entity)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Insertable(entity).ExecuteCommandIdentityIntoEntity();
                return result;
            }
        }

        public int AddBatch(IEnumerable<T> list)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Insertable(list.ToArray()).ExecuteCommand();
                return result;
            }
        }

        public int Delete(T entity)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Deleteable(entity).ExecuteCommand();
                return result;
            }
        }

        public int Delete(IEnumerable<T> list)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Deleteable<T>(list).ExecuteCommand();
                return result;
            }
        }

        public int Delete(int id)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Deleteable<T>(id).ExecuteCommand();
                return result;
            }
            
        }

        public int Delete(int[] ids)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Deleteable<T>(ids).ExecuteCommand();
                return result;
            }
        }

        public int Delete(IEnumerable<int> ids)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Deleteable<T>(ids).ExecuteCommand();
                return result;
            }
        }

        public int Modify(T entity)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Updateable(entity).ExecuteCommand();
                return result;
            }
        }

        public int ModifyBatch(List<T> list)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Updateable(list).ExecuteCommand();
                return result;
            }
        }

        public IEnumerable<T> QueryAll()
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Queryable<T>().ToList();
                return result;
            }
        }

        public IEnumerable<T> QueryTop(int top)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Queryable<T>().Take(top).ToList();
                return result;
            }
        }

        public T QueryById(int id)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Queryable<T>().InSingle(id);
                return result;
            }
        }

        public IEnumerable<T> QueryPage(Expression<Func<T, object>> orderBy, int orderType, int pageIndex, int pageSize, ref int totalCount)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Queryable<T>().OrderBy(orderBy, (OrderByType)orderType).ToPageList(pageIndex, pageSize, ref totalCount);
                return result;
            }
        }

        public IEnumerable<T> QueryByIf(Expression<Func<T, bool>> where)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                if (where != null)
                {
                    var list = db.Queryable<T>().WhereIF(true, where).ToList();
                    return list;
                }
                else
                {
                    var list = db.Queryable<T>().ToList();
                    return list;
                }
            }
            
        }

        public IEnumerable<T> QueryByIfPage(Expression<Func<T, object>> orderBy, int orderType, Expression<Func<T, bool>> where, int pageIndex, int pageSize, ref int totalCount)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var list = db.Queryable<T>().WhereIF(true, where).OrderBy(orderBy, (OrderByType)orderType).ToPageList(pageIndex, pageSize, ref totalCount);
                return list;
            }
        }
    }
}

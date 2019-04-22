using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;
using TrumguSignalR.Util.Serialize;

namespace TrumguSignalR.Cache
{
    public class RedisCacheManager:ICache
    {
        private static ConfigurationOptions _redisConnectionString;
        private static volatile ConnectionMultiplexer _instance;
        private static readonly object Locker = new object();
        private readonly IDatabase _redisDb;

        public RedisCacheManager(string conString,int dbIndex=1)
        {
            if (string.IsNullOrWhiteSpace(conString))
            {
                throw new ArgumentException("redis config is empty", nameof(conString));
            }
            //处理链接字符串
            var configurationOptions = ConfigurationOptions.Parse(conString);
            _redisConnectionString = configurationOptions;
            _instance = Instance();
            _redisDb = _instance.GetDatabase(dbIndex);
        }

        /// <summary>
        /// 单例模式 获取redis实例
        /// </summary>
        /// <returns></returns>
        public static ConnectionMultiplexer Instance()
        {
            if (_instance != null && _instance.IsConnected)
            {
                return _instance;
            }

            lock (Locker)
            {
                _instance?.Dispose();

                _instance = ConnectionMultiplexer.Connect(_redisConnectionString);
                
            }

            return _instance;

        }

       

        public T Get<T>(string key) where T : class
        {
            if (!_redisDb.KeyExists(key))
            {
                return default(T);
            }

            var value = _redisDb.StringGet(key);
            return value.HasValue ? Deserialize<T>(value) : default(T);
        }

        public void Set(string key, object value, TimeSpan cacheTime)
        {
            if (value != null)
            {
                _redisDb.StringSet(key, Serialize(value), cacheTime);
            }
        }

        public bool Contains(string key)
        {
            return _redisDb.KeyExists(key);
        }

        public void Remove(string key)
        {
            if (!_redisDb.KeyExists(key))
            {
                return;
            }
            _redisDb.KeyDelete(key);
        }

        public void Clear()
        {
            foreach (var endPoint in Instance().GetEndPoints())
            {
                var server = Instance().GetServer(endPoint);
                foreach (var key in server.Keys())
                {
                    _redisDb.KeyDelete(key);
                }
            }
        }

        /// <summary>
        /// 从hash表获取Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey) where T : class
        {
            var redisValue = _redisDb.HashGet(key, dataKey);
            var bytes = Convert.FromBase64String(redisValue);
            var model = BinarySerializeHelper.DeserializeObject2(bytes) as T;
            return model;
        }

        


        private static T Deserialize<T>(byte[] value)
        {
            if (value == null)
            {
                return default(T);
            }
            var jsonString = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        private static byte[] Serialize(object item)
        {
            var jsonString = JsonConvert.SerializeObject(item);
            return Encoding.UTF8.GetBytes(jsonString);
        }
    }
}

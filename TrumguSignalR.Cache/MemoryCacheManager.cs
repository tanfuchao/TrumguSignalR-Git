using System;
using System.Runtime.Caching;

namespace TrumguSignalR.Cache
{
    public class MemoryCacheManager : ICache
    {
        public T Get<T>(string key) where T : class
        {
            return (T)MemoryCache.Default.Get(key);
        }

        public void Set(string key, object value, TimeSpan cacheTime)
        {
            MemoryCache.Default.Add(key, value, new CacheItemPolicy { SlidingExpiration = cacheTime });
        }

        public bool Contains(string key)
        {
            return MemoryCache.Default.Contains(key);
        }

        public void Remove(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        public void Clear()
        {
            foreach (var item in MemoryCache.Default)
            {
                Remove(item.Key);
            }
        }

        public T HashGet<T>(string key, string dataKey) where T : class
        {
            return default(T);
        }
    }
}

using System;

namespace TrumguSignalR.Cache
{
    /// <summary>
    /// 创建人：谭福超
    /// 日 期：2018年12月20日
    /// 描 述：定义缓存接口
    /// </summary>
    public interface ICache
    {
        T Get<T>(string key) where T : class;

        void Set(string key, object value, TimeSpan cacheTime);

        bool Contains(string key);

        void Remove(string key);

        void Clear();

        T HashGet<T>(string key, string dataKey) where T : class;
    }
}

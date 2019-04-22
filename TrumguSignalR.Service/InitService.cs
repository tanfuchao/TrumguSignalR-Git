using System;
using System.Collections.Generic;
using TrumguSignalR.Cache;
using TrumguSignalR.Log;
using TrumguSignalR.Model;
using TrumguSignalR.MySql.DAL;
using TrumguSignalR.MySql.IDAL;
using TrumguSignalR.Util.Config;

/**
 * 思路
 * 首先 每次有链接进来 都存入到onlineList 静态变量吧 不适合存cache
 * 因为存cache 一个用户下线时 我没办法更新 onlineList 删除了 又无法把没下线的用户重新加进去
 * 循环 onlineList 在redis中取到userid 将这一对一的关系 存储到cache signal-userid
 * 从mysql中 各种循环 得到 所有user-codeList 存入cache 永久存
 * 方法2 循环在线用户 可以得到 signal-codeList 存入cache
 * 新用户进来signal-userid cache 增加
 * 在执行一次方法2 更新code-signalList
 * 用户断开链接时 onlineList-1 在更新signal-userid
 * 在执行一次方法2 更新code-signalList
 * 用户添加自选股时
 * 要更新对应的user-codeList 这就要针对用户重新查了
 * 删除自选股也一样
 * 继续 方法2这里有出入 我应该时存signal-codeList
 * 信号推送时 比如同时出了123信号 循环在线用户
 * c.user(s1).list(1,2)
 * c.user(s2).list(2.3)
 * 实际s1 里面存有1,2,4 这里和出的信号取交集 没有交集就不推送
 * 只推交集
 * 微信思路
 * onlineList wx_123guid
 * wx_guid-userid cache
 * userid-codeList
 *
 *
*/
namespace TrumguSignalR.Service
{
    /// <summary>
    /// 作者 谭福超
    /// 时间 2019年3月29日
    /// 作用 一些初始化的操作我打算全放在这里
    /// </summary>
    public class InitService
    {
        private readonly string _redisOnline = "redisOnline";


        private readonly ICache _cache = new RedisCacheManager(Config.GetValue("ConStringRedis"));
        private readonly ICache _sessionCache = new RedisCacheManager(Config.GetValue("ConStringRedis"), 0);
        private readonly ITbfT0CombinationDal _dao = new TbfT0CombinationDal();
        private readonly ITbfT0CombinationFundDal _fundDao = new TbfT0CombinationFundDal();
        private readonly IStockTraCalendarDal _calendarDal = new StockTraCalendarDal();

        //初始化时移除redis中的在线用户
        public void InitOnline()
        {
            _cache.Remove("redisOnline");
        }

        /// <summary>
        /// 判断是否是交易日
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsMarketDay(DateTime date)
        {
            return _calendarDal.IsMarketDay(date);
        }

        /// <summary>
        /// 新增在线用户,也就是再hub中建立链接时调用
        /// </summary>
        /// <param name="onlineUser"></param>
        public void AddOnlineCache(string onlineUser)
        {
            IList<string> onlineList;
            if (_cache.Contains(_redisOnline))
            {
                onlineList = _cache.Get<List<string>>(_redisOnline);
                _cache.Remove(_redisOnline);
            }
            else
            {
                onlineList = new List<string>();
            }
            onlineList.Add(onlineUser);
            _cache.Set(_redisOnline, onlineList, TimeSpan.FromDays(1));
        }

        /// <summary>
        /// 用户下线,更新在线用户,也就是再hub中断开链接调用
        /// </summary>
        /// <param name="onlineUser"></param>
        public void DelOnlineCache(string onlineUser)
        {
            if (_cache.Contains(_redisOnline))
            {
                IList<string> onlineList = _cache.Get<List<string>>(_redisOnline);
                _cache.Remove(_redisOnline);
                onlineList.Remove(_redisOnline);
                _cache.Set(_redisOnline, onlineList, TimeSpan.FromDays(1));
            }
            else
            {
                var exception = new ApplicationException("redis db1中不存在 在线用户缓存");
                LogWrite.WriteLogError(exception);
            }

        }

        /// <summary>
        /// 获取当前在线用户列表
        /// </summary>
        /// <returns></returns>
        public List<string> GetOnlineCache()
        {
            var onlineCacheList = _cache.Contains(_redisOnline)
                ? _cache.Get<List<string>>(_redisOnline)
                : null;

            return onlineCacheList;
        }

        /// <summary>
        /// 这个方法执行时间很长
        /// 将mysql中 user和codeList的关系 存入了缓存
        /// user-codeList
        /// </summary>
        public void CreateUserIdCodeListCache()
        {
            LogWrite.WriteLogInfo("-----初始化mysql数据库中的user-codeList缓存-----------");
            //所有去重过的用户
            var allCreateUserId = _dao.GetAllCreateUserId();
            foreach (var userId in allCreateUserId)
            {
                List<string> codeList = new List<string>();
                //得到 该用户 所有创建的组合
                List<string> combinationGuidList = _dao.GetCombinationByCreateId(userId);
                foreach (var combinationGuid in combinationGuidList)
                {
                    //得到 该组合的 股票代码List
                    var stockCodeList = _fundDao.GetStockCodeByGuidList(combinationGuid);
                    codeList.AddRange(stockCodeList);
                }

                var contains = _cache.Contains(userId.ToString());
                if (contains)
                {
                    _cache.Remove(userId.ToString());
                }
                _cache.Set(userId.ToString(), codeList, TimeSpan.FromDays(365));



            }
            LogWrite.WriteLogInfo("最长缓存结束了,已经将所有的userId-codeList关联存入缓存");
        }



        //在线用户发生改变时
        public void ChangeOnline()
        {
            LogWrite.WriteLogInfo("---在线用户发生了变化,更新online-user缓存,更新signal-codeList缓存Start---");
            CreateSignalIdUserIdCache();
            CreateSignalCodeListCache();
            LogWrite.WriteLogInfo("---在线用户发生了变化,更新signal-userId缓存,更新signal-codeList缓存end---");
        }

        //自选股发生改变时
        public void ChangeOwnCodeList(string sessionId)
        {
            var userId = GetUserIdByRedis(sessionId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                LogWrite.WriteLogInfo($"---------未能在sessionRedis中获取到用户的UserId,对应的sessionId为{sessionId}------");
                return;
            }
            List<string> codeList = new List<string>();
            //得到 该用户 所有创建的组合
            List<string> combinationGuidList = _dao.GetCombinationByCreateId(Convert.ToInt32(userId));
            foreach (var combinationGuid in combinationGuidList)
            {
                //得到 该组合的 股票代码List
                var stockCodeList = _fundDao.GetStockCodeByGuidList(combinationGuid);
                codeList.AddRange(stockCodeList);
            }

            var contains = _cache.Contains(userId);
            if (contains)
            {
                _cache.Remove(userId);
            }
            _cache.Set(userId, codeList, TimeSpan.FromDays(365));
        }

        //根据某一个在线用户 获取他所关注的股票codeList
        public List<string> GetOnlineCodeList(string online)
        {
            string key = $"push{online}";
            var contains = _cache.Contains(key);
            if (!contains)
            {
                LogWrite.WriteLogError(new Exception($"redis db1在线用户{online}不存在push_online-codeList缓存"));
            }

            var codeList = _cache.Get<List<string>>(key);
            return codeList;
        }

        #region 私有方法
        /// <summary>
        /// 根据在线用户的静态变量
        /// 将在线用户和userId缓存
        /// signal-userId
        /// </summary>
        private void CreateSignalIdUserIdCache()
        {
            LogWrite.WriteLogInfo("--------------创建online-user缓存--------");
            var onlineList = GetOnlineCache();
            if (onlineList == null || onlineList.Count <= 0)
            {
                return;
            }

            foreach (var online in onlineList)
            {
                string userId = GetUserIdByRedis(online);
                var contains = _cache.Contains(online);
                if (contains)
                {
                    _cache.Remove(online);
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    _cache.Set(online, userId, TimeSpan.FromDays(1));
                }
            }
        }

        /// <summary>
        /// 构建用来推送的缓存
        /// push+signal-codeList
        /// </summary>
        private void CreateSignalCodeListCache()
        {
            LogWrite.WriteLogInfo("--------------创建推送缓存 push_online-codeList缓存--------");

            var onlineList = GetOnlineCache();
            if (onlineList == null || onlineList.Count <= 0)
            {
                return;
            }

            foreach (var online in onlineList)
            {
                if (!_cache.Contains(online))
                {
                    LogWrite.WriteLogError(new ApplicationException($"构建推送缓存时,缓存中不存在当前session:{online}为key的 online-userId缓存"));
                    return;
                }

                var userId = _cache.Get<string>(online);
                if (!_cache.Contains(userId))
                {
                    LogWrite.WriteLogError(new Exception($"构建推送缓存时,缓存中不存在用户userid{userId}为key的 user-codeList缓存"));
                    return;
                }

                var codeList = _cache.Get<List<string>>(userId);
                //online的key已经存在,所以我要加个前缀
                string key = $"push{online}";
                var contains = _cache.Contains(key);
                if (contains)
                {
                    _cache.Remove(key);
                }

                if (codeList != null && codeList.Count > 0)
                {
                    _cache.Set(key, codeList, TimeSpan.FromDays(1));
                }

            }
        }

        public string GetUserIdByRedis(string sessionId)
        {
            LogWrite.WriteLogInfo("--------------从sessionRedis中得到userID--------");
            if (_sessionCache.Contains(sessionId))
            {
                var sessionKey = sessionId.Contains("WX_") ? "user_info" : "UserInfo";
                t_bf_sys_userObj userInfo = _sessionCache.HashGet<t_bf_sys_userObj>(sessionId, sessionKey);
                if (userInfo == null)
                {
                    LogWrite.WriteLogError(new ApplicationException($"sessionRedis没有成功解析{sessionId}的用户信息"));
                    return null;
                }
                string userId = userInfo.id.ToString();
                LogWrite.WriteLogInfo($"从sessionRedis获取到了{sessionId}的userId:{userId}");
                return userId;
            }

            LogWrite.WriteLogError(new ApplicationException($"sessionRedis中没有找到key为:{sessionId}的用户信息缓存"));
            return null;


        }




        #endregion

    }
}

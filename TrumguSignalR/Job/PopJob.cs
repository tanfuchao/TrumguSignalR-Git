using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Quartz;
using TrumguSignalR.Cache;
using TrumguSignalR.Log;
using TrumguSignalR.Model.HashModel;
using TrumguSignalR.Model.MongoModel;
using TrumguSignalR.RabbitMQ;
using TrumguSignalR.Service;

namespace TrumguSignalR.Job
{
    public class PopJob : RabbitProducer, IJob
    {
        private static DateTime _time1 = DateTime.Now.AddHours(8);
        private static readonly MongoService MongoService = new MongoService();
        private static readonly InitService InitService = new InitService();

        public Task Execute(IJobExecutionContext context)
        {
            var isMarketDay = InitService.IsMarketDay(DateTime.Now.Date);

            if (!isMarketDay)
            {
                Console.WriteLine("非交易日,不进行信号推送");
                LogWrite.WriteLogInfo("非交易日,不进行信号推送");
                return Task.FromResult(0);
            }

            Console.WriteLine("右下角推送任务");
            LogWrite.WriteLogInfo($"------------右下角推送任务,时间:{DateTime.Now.ToLongTimeString()}---------------");

            /*
             * 思路
             * 首先 扫库 这应该放到其他类中
             * 循环在线用户
             * 对比扫库中的List和在线用户对应的codeList
             * 取交集推送
             *
             * 修改思路 不用count值作为推送条件 用时间作为推送筛选条件
             */
            DateTime time2 = DateTime.Now.AddHours(8);
            LogWrite.WriteLogInfo($"time1:{_time1}");
            LogWrite.WriteLogInfo($"time2:{time2}");
            var signalNowList = MongoService.GetStockSignalSingleByTime(_time1, time2);
            if (signalNowList != null && signalNowList.Count > 0)
            {
                #region debug代码,影响应能
                foreach (var stockSignalSingle in signalNowList)
                {
                    LogWrite.WriteLogInfo($"signalNowList-mongodb中新增的 未经用户过滤的信号:{stockSignalSingle.code}");
                } 
                #endregion


                var onlineCache = InitService.GetOnlineCache();
                if (onlineCache==null || onlineCache.Count <= 0)
                {
                    return Task.FromResult(0);
                }
                LogWrite.WriteLogInfo($"InitService.OnlineList-在线用户列表:{onlineCache.Count}");
                foreach (var online in onlineCache)
                {
                    LogWrite.WriteLogInfo($"online-循环的在线用户:{online}");

                    var codeList = InitService.GetOnlineCodeList(online);
                    LogWrite.WriteLogInfo($"codeList-用户关注的信号个数:{codeList.Count}");
                    #region debug代码 影响性能
                    foreach (var code in codeList)
                    {
                        LogWrite.WriteLogInfo($"当前用户:{online}关注的信号code:{code}");
                    } 
                    #endregion
                    //signalNowList和codeList取交集
                    var nowCodeList = signalNowList.Select(m => m.code).ToList();
                    //得到了交集
                    if (codeList.Count <= 0 || nowCodeList.Count <= 0)
                    {
                        continue;
                    }
                    var intersect = codeList.Intersect(nowCodeList).ToList();
                    #region debug代码 影响性能 交集信号
                    foreach (var code in intersect)
                    {
                        LogWrite.WriteLogInfo($"交集信号有{code}");
                    }
                    #endregion
                    if (intersect.Count <= 0)
                    {
                        continue;
                    }
                    //构建推送架构
                    List<HashPop> hashPops = MongoService.CreatePopList(signalNowList, intersect);
                    LogWrite.WriteLogInfo($"hashPops-构建好的准备推送信号个数:{nowCodeList.Count}");

                    if (hashPops == null || hashPops.Count <= 0)
                    {
                        continue;
                    }
                    Console.WriteLine($"信号推送:用户{online},信号{hashPops.Count}");
                    LogWrite.WriteLogInfo("---------信号推送start-------------");
                    LogWrite.WriteLogInfo($"用户:{online}");
                    foreach (var pop in hashPops)
                    {
                        LogWrite.WriteLogInfo($"信号代码:{pop.Code}");
                    }
                    LogWrite.WriteLogInfo("---------信号推送end-------------");
                    GlobalHost.ConnectionManager.GetHubContext<SignalHub>().Clients
                        .User(online)
                        .alert_signal(hashPops);
                }
            }

            _time1 = time2;

            return Task.FromResult(0);
        }

        //测试数据方法
        public Task Execute2(IJobExecutionContext context)
        {
            Console.WriteLine("PopSend任务调度");
            LogWrite.WriteLogInfo("PopSend任务调度");
            #region 测试

            var stockSignalSingles = MongoService.GetAllStockSignalSingle();
            var model1 = stockSignalSingles.FindLast(m => m.code == "000001");
            var model2 = stockSignalSingles.FindLast(m => m.code == "600000");
            var model3 = stockSignalSingles.FindLast(m => m.code == "000063");
            var model4 = stockSignalSingles.FindLast(m => m.code == "603999");
            var model5 = stockSignalSingles.FindLast(m => m.code == "603998");
            List<stock_signal_single> list = new List<stock_signal_single>
            {
                model1,model2,model3,model4,model5
            };
            foreach (var online in InitService.GetOnlineCache())
            {
                var memoryCacheManager = new MemoryCacheManager();
                var cacheUserId = memoryCacheManager.Get<string>(online);
                Console.WriteLine($"signal:{online}-userId:{cacheUserId}");
                var cacheCodeList = memoryCacheManager.Get<List<string>>(cacheUserId);
                foreach (var s in cacheCodeList)
                {
                    Console.WriteLine($"user:{cacheUserId}-codeList{s}");
                }

                var cacheCodeList1 = memoryCacheManager.Get<List<string>>($"push{online}");
                Console.WriteLine($"signal:push{online}-codeList{cacheCodeList1.Count}");
                var codeList = InitService.GetOnlineCodeList(online);
                //signalNowList和codeList取交集
                var nowCodeList = list.Select(m => m.code).ToList();
                //得到了交集
                if (codeList == null || codeList.Count <= 0 || nowCodeList.Count <= 0) continue;
                var intersect = codeList.Intersect(nowCodeList).ToList();
                if (intersect.Count <= 0) continue;
                //构建推送架构
                List<HashPop> hashPops = MongoService.CreatePopList(list, intersect);
                if (hashPops == null || hashPops.Count <= 0) continue;
                Console.WriteLine($"信号推送:用户{online},信号{hashPops.Count}");
                //                LogWrite.WriteLogInfo("---------信号推送start-------------");
                //                LogWrite.WriteLogInfo($"用户:{online}");
                //                foreach (var pop in hashPops)
                //                {
                //                    LogWrite.WriteLogInfo($"信号代码:{pop.Code}");
                //                }
                //                LogWrite.WriteLogInfo("---------信号推送end-------------");
                GlobalHost.ConnectionManager.GetHubContext<SignalHub>().Clients
                    .User(online)
                    .alert_signal(hashPops);
            }

            #endregion
            return Task.FromResult(0);
        }
    }
}

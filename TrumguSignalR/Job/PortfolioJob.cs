using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Quartz;
using TrumguSignalR.Log;
using TrumguSignalR.Model.HashModel;
using TrumguSignalR.Service;

namespace TrumguSignalR.Job
{
    public class PortfolioJob:IJob
    {
        private static readonly MongoService MongoService = new MongoService();
        private static readonly InitService InitService = new InitService();

        public Task Execute(IJobExecutionContext context)
        {
            var isMarketDay = InitService.IsMarketDay(DateTime.Now.Date);


            if (!isMarketDay)
            {
                Console.WriteLine("非交易日,不进行自选股推送");
                LogWrite.WriteLogInfo("非交易日,不进行自选股推送");
                return Task.FromResult(0);
            }
            Console.WriteLine("首页自选股任务调度");
            LogWrite.WriteLogInfo($"------------首页自选股任务调度,时间:{DateTime.Now.ToLongTimeString()}---------------");

            /*
             * 新的思路
             * 循环在线用户
             * 通过signal-codeList缓存
             * 找到每个用户需要推送的codeList
             * 在循环codeList 构建要推送的新消息 得到一个List 这应该是放在另一个方法中
             * foreach(online){c.user(online).alert(codeList)}
             */
            var onlineCache = InitService.GetOnlineCache();
            if (onlineCache==null || onlineCache.Count <= 0)
            {
                return Task.FromResult(0);
            }
            foreach (var online in onlineCache)
            {
                List<string> codeList = InitService.GetOnlineCodeList(online);
                if (codeList == null || codeList.Count <= 0) continue;
                List<HashPortfolio> signalList = MongoService.GetPortfolioList(codeList);
                if (signalList == null || signalList.Count <= 0) continue;
                LogWrite.WriteLogInfo($"------------首页自选股推送了Start,时间:{DateTime.Now.ToLongTimeString()}---------------");
                GlobalHost.ConnectionManager.GetHubContext<SignalHub>().Clients
                    .User(online)
                    .home_follow(signalList);
                LogWrite.WriteLogInfo($"接收首页自选股的用户:{online}");
                LogWrite.WriteLogInfo($"接收首页自选股的信号数量:{signalList.Count}");
                //详细日志,当没有问题是,注释掉下面的for循环提高性能
                foreach (var hashPortfolio in signalList)
                {
                    LogWrite.WriteLogInfo($"接收首页自选股的信号代码:{hashPortfolio.StockCode}");
                }

                LogWrite.WriteLogInfo($"------------首页自选股推送了End,时间:{DateTime.Now.ToLongTimeString()}---------------");

            }

            return Task.FromResult(0);
        }
    }
}

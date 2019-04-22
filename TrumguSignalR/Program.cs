using System;
using Microsoft.Owin.Hosting;
using Quartz;
using Quartz.Impl;
using TrumguSignalR.Job;
using TrumguSignalR.Log;
using TrumguSignalR.Service;
using TrumguSignalR.Util.Config;

namespace TrumguSignalR
{
    public class Program
    {

        
        public static void Main(string[] args)
        {
            //删除rabbitMQ积压的数据
            WxPopJob.DeleteQueue();
            Console.WriteLine("删除rabbitMQ积压的数据");
            InitService initService = new InitService();
            //移除在线用户
            initService.InitOnline();
            //重启时,更新所有 userid-codeList的缓存
            initService.CreateUserIdCodeListCache();
            Console.WriteLine("更新所有 userid-codeList的缓存---完毕");
            Console.WriteLine("主进程开始..");
            LogWrite.WriteLogInfo("主进程开始");
            
            var url = Config.GetValue("SignalRUrl");
            LogWrite.WriteLogInfo($"服务地址{url}");
            Console.WriteLine($"服务地址{url}");
            using (WebApp.Start(url))
            {
                try
                {
                    LogWrite.WriteLogInfo("服务端口开启成功");
                    Console.WriteLine("服务端口开启成功");
                    QuartzJob();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    LogWrite.WriteLogError(e);
                }
                finally
                {
                    Console.ReadKey();
                }
            }
        }

        public static void QuartzJob()
        {
            Console.WriteLine("QuartzJob任务调度开始工作");
            
            #region 任务
            //任务1
            IJobDetail popJob = JobBuilder.Create<PopJob>()
                .WithIdentity("popJob", "popJob")
                .Build();
            //任务2
            IJobDetail portfolioJob = JobBuilder.Create<PortfolioJob>()
                .WithIdentity("portfolioJob", "portfolioJob")
                .Build();
            //任务3
            IJobDetail rangeJob = JobBuilder.Create<RangeJob>()
                .WithIdentity("rangeJob", "rangeJob")
                .Build();
            //任务4
            IJobDetail wxPopJob = JobBuilder.Create<WxPopJob>()
                .WithIdentity("wxPopJob", "wxPopJob")
                .Build();
            #endregion


            #region 触发器

            //触发器1 StartNow()和Cron不能同时存在,StartNow会失效
            ITrigger popTrigger = TriggerBuilder.Create()
                .WithIdentity("popTrigger", "popTrigger")
                .WithCronSchedule(Config.GetValue("PopCron"))
                .Build();

            //触发器2
            ITrigger portfolioTrigger = TriggerBuilder.Create()
                .WithIdentity("portfolioTrigger", "portfolioTrigger")
                .WithCronSchedule(Config.GetValue("PortfolioCron"))
                .Build();

            //触发器3
            ITrigger rangeTrigger = TriggerBuilder.Create()
                .WithIdentity("rangeTrigger", "rangeTrigger")
                .WithCronSchedule(Config.GetValue("RangeCron"))
                .Build();

            //触发器4
            ITrigger wxPopTrigger = TriggerBuilder.Create()
                .WithIdentity("wxPopTrigger", "wxPopTrigger")
                .WithCronSchedule(Config.GetValue("WxPopCron"))
                .Build();

            #endregion

            

            #region 调度器

            //调度器
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = schedulerFactory.GetScheduler().Result;

            scheduler.ScheduleJob(popJob, popTrigger);
            scheduler.ScheduleJob(rangeJob, rangeTrigger);
            scheduler.ScheduleJob(portfolioJob, portfolioTrigger); 
            scheduler.ScheduleJob(wxPopJob, wxPopTrigger); 

            #endregion

            scheduler.Start();
            var popNextTimeUtc = popTrigger.GetNextFireTimeUtc().GetValueOrDefault().DateTime;
            var portfolioNextTimeUtc = portfolioTrigger.GetNextFireTimeUtc().GetValueOrDefault().DateTime;
            var rangeNextTimeUtc = rangeTrigger.GetNextFireTimeUtc().GetValueOrDefault().DateTime;
            var wxPopNextTimeUtc = wxPopTrigger.GetNextFireTimeUtc().GetValueOrDefault().DateTime;
            var popNextTime = TimeZone.CurrentTimeZone.ToLocalTime(popNextTimeUtc);
            var portfolioNextTime = TimeZone.CurrentTimeZone.ToLocalTime(portfolioNextTimeUtc);
            var rangeNextTime = TimeZone.CurrentTimeZone.ToLocalTime(rangeNextTimeUtc);
            var wxPopNextTime = TimeZone.CurrentTimeZone.ToLocalTime(wxPopNextTimeUtc);
            Console.WriteLine($"信号推送任务下次执行时间:{popNextTime}");
            Console.WriteLine($"首页自选股任务下次执行时间:{portfolioNextTime}");
            Console.WriteLine($"涨跌幅任务下次执行时间:{rangeNextTime}");
            Console.WriteLine($"微信推送任务下次执行时间:{wxPopNextTime}");
        }

    }
}

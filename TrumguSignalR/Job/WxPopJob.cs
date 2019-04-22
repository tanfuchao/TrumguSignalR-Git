using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;
using TrumguSignalR.Log;
using TrumguSignalR.Model.MongoModel;
using TrumguSignalR.RabbitMQ;
using TrumguSignalR.Service;
using TrumguSignalR.Util.Serialize;
using IConnection = RabbitMQ.Client.IConnection;

namespace TrumguSignalR.Job
{
    public class WxPopJob : RabbitProducer, IJob
    {
       
        private static readonly MongoService MongoService = new MongoService();
        private static readonly InitService InitService = new InitService();

        private static DateTime _time1 = DateTime.Now.AddHours(8);


        public Task Execute(IJobExecutionContext context)
        {

            var isMarketDay = InitService.IsMarketDay(DateTime.Now.Date);


            if (!isMarketDay)
            {
                Console.WriteLine("非交易日,不进行微信推送");
                LogWrite.WriteLogInfo("非交易日,不进行微信推送");
                return Task.FromResult(0);
            }
            Console.WriteLine("微信推送");

            #region 正式数据
            /*
             * 思路
             * 首先 扫库 这应该放到其他类中
             * 循环在线用户
             * 对比扫库中的List和在线用户对应的codeList
             * 取交集推送
             */
            DateTime time2 = DateTime.Now.AddHours(8);
            var signalNowList = MongoService.GetStockSignalSingleByTime(_time1, time2);
            
            if (signalNowList!=null && signalNowList.Count> 0)
            {
                foreach (var single in signalNowList)
                {
                    single.time = single.time.AddHours(-8);
                }

                Console.WriteLine($"时间:{signalNowList[0].time}");
                DirectExchangeSendMsg(signalNowList);
            }
            _time1 = time2;
            #endregion
            return Task.FromResult(0);
        }

        //测试方法
//        public Task Execute(IJobExecutionContext context)
//        {
//            Console.WriteLine("微信推送");
//
//            #region 测试
//            var stockSignalSingles = MongoService.GetAllStockSignalSingle();
//            var model1 = stockSignalSingles.FirstOrDefault();
//            var model2 = stockSignalSingles.Last();
//            List<stock_signal_single> list = new List<stock_signal_single>
//            {
//                model1,model2
//            };
//            DirectExchangeSendMsg(list);
//            #endregion
//
//
//            return Task.FromResult(0);
//        }

        public static void DirectExchangeSendMsg(List<stock_signal_single> signalNowList)
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(QueueName, ExchangeName, routingKey: "signalList");
                    var props = channel.CreateBasicProperties();
                    
                    props.Persistent = true;
                    var listToJson = JsonConvert.SerializeObject(signalNowList);
                    var msgBody = BinarySerializeHelper.SerializeObject(listToJson);
                    
                    channel.BasicPublish(exchange: ExchangeName, routingKey: "signalList", basicProperties: props, body: msgBody);
                }
            }
        }

        /// <summary>
        /// 每次重启时,清空队列
        /// </summary>
        public static void DeleteQueue()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    try
                    {
                        channel.QueueDelete(QueueName, true, false);
                    }
                    catch (Exception e)
                    {
                        LogWrite.WriteLogInfo($"重启时,存在消费者,不删除队列,异常{e.Message}");
                    }
                    
                    
                }
            }
        }
    }
}

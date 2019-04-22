using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Quartz;
using RabbitMQ.Client;
using TrumguSignalR.Log;
using TrumguSignalR.RabbitMQ;
using TrumguSignalR.Service;
using TrumguSignalR.Util.Serialize;
using IConnection = RabbitMQ.Client.IConnection;

namespace TrumguSignalR.Job
{
    public class RangeJob: RabbitProducer,IJob
    {
        private static readonly MongoService Service = new MongoService();
        private static readonly InitService InitService = new InitService();

        public Task Execute(IJobExecutionContext context)
        {
            var isMarketDay = InitService.IsMarketDay(DateTime.Now.Date);

            if (!isMarketDay)
            {
                Console.WriteLine("非交易日,不进行涨跌幅推送");
                LogWrite.WriteLogInfo("非交易日,不进行涨跌幅推送");
                return Task.FromResult(0);
            }
            Console.WriteLine("涨跌幅任务调度");
            LogWrite.WriteLogInfo($"------------涨跌幅任务调度,时间:{DateTime.Now.ToLongTimeString()}---------------");
            var chgModel = Service.GetChgData();
            GlobalHost.ConnectionManager.GetHubContext<SignalHub>().Clients
                .All
                .upordown(chgModel);
            return Task.FromResult(0);
        }

        public static void DirectExchangeSendMsg()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(QueueName, ExchangeName, routingKey: "key1");
//                    channel.QueueDeclare(QueueName2, durable: true, autoDelete: false, exclusive: false, arguments: null);
//                    channel.QueueBind(QueueName2, ExchangeName, routingKey: "key2");

                    var props = channel.CreateBasicProperties();
                    props.Persistent = true;
                    var chgModel = Service.GetChgData();
                    var msgBody = BinarySerializeHelper.SerializeObject(chgModel);
//                    var msgBody = Encoding.UTF8.GetBytes(bytes);
                    channel.BasicPublish(exchange: ExchangeName, routingKey: "key1", basicProperties: props, body: msgBody);
                }
            }
        }
    }
}

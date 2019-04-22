using System;
using RabbitMQ.Client;
using TrumguSignalR.Util.Config;

namespace TrumguSignalR.RabbitMQ
{
    public class RabbitProducer
    {
        public static readonly ConnectionFactory RabbitMqFactory = new ConnectionFactory()
        {
            HostName = Config.GetValue("RabbitHostName"),
            UserName = Config.GetValue("RabbitUserName"),
            Password = Config.GetValue("RabbitPassword"),
            VirtualHost = Config.GetValue("RabbitVirtualHost"),
            Port = Convert.ToInt32(Config.GetValue("RabbitPort"))
        };

        /// <summary>
        /// 交换机名称
        /// </summary>
        public const string ExchangeName = "exchange";

        //队列名称
        public const string QueueName = "queue";
    }
}

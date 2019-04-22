using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TrumguSignalR.Model.MongoModel;

namespace RabbitConsumer
{
    public class SignalCustomer
    {

        private static readonly ConnectionFactory RabbitMqFactory = new ConnectionFactory()
        {
            HostName = "127.0.0.1",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            Port = 5672
        };

        private static readonly string ExchangeName = "exchange";

        //队列名称
        private static readonly string QueueName = "queue";

        //返回链接状态
        public static bool GetRabbitMqConnState()
        {
            IConnection conn = RabbitMqFactory.CreateConnection();
            return conn.IsOpen;
        }

        //返回协议状态
        public static bool GetRabbitMqModelState()
        {
            if (!GetRabbitMqConnState()) return false;
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                IModel channel = conn.CreateModel();
                return channel.IsOpen;
            }

        }

        /// <summary>
        /// 路由模式消费者入口
        /// </summary>
        public static void DirectAcceptExchangeEvent()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(ExchangeName, "direct", durable: true, autoDelete: false, arguments: null);
                    channel.QueueDeclare(QueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(QueueName, ExchangeName, routingKey: "signalList");
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
//                        Console.WriteLine(ea.Body);
                        //ea.Body这里是个字节数组
                        var byteToJson = DeserializeObject(ea.Body).ToString();
                        List<stock_signal_single_ex> list = JsonConvert.DeserializeObject<List<stock_signal_single_ex>>(byteToJson);
                        foreach (var stockSignalSingle in list)
                        {
                            Console.WriteLine($"模板信号代码{stockSignalSingle.code},时间{stockSignalSingle.time}");
                        }
                    };
                    channel.BasicConsume(QueueName, true, consumer: consumer);
       
                    Console.ReadKey(); //这句话必不可少 为了让rabbit不再一瞬间结束 需要阻塞这个进程
                }
            }
        }

        private static object DeserializeObject(byte[] pBytes)
        {
            object _newOjb = null;
            if (pBytes == null)
                return _newOjb;
            System.IO.MemoryStream _memory = new System.IO.MemoryStream(pBytes);
            _memory.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            _newOjb = formatter.Deserialize(_memory);
            _memory.Close();
            return _newOjb;
        }




    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitConsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            #region 检测RabbitMQ服务器
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("-------------------- Rabbit Service Starting --------------------\r\n");
            if (SignalCustomer.GetRabbitMqConnState())
            {
                if (SignalCustomer.GetRabbitMqModelState())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("->Rabbit Service Start Success\r\n");
                    //开启一个线程 让子线程一直执行消费者功能
                    Task.Factory.StartNew(SignalCustomer.DirectAcceptExchangeEvent);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("->Rabbit Model Service Start Fail\r\n");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("->Rabbit Service Start Fail\r\n");
            }

            Console.ReadKey();

            #endregion
        }
    }
}

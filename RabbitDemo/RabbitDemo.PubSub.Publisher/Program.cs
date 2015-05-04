using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitDemo.Utilities;
using RabbitMQ.Client;

namespace RabbitDemo.PubSub.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            var rnd = new Random();
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare("orders", ExchangeType.Fanout, true);

                    while (true)
                    {
                        Console.Write("Number of orders to create: ");
                        var input = Console.ReadLine();
                        if (string.IsNullOrEmpty(input))
                            break;
                        int numberOfOrders;
                        int.TryParse(input, out numberOfOrders);

                        for (int i = 0; i < numberOfOrders; i++)
                        {
                            decimal orderAmount = rnd.Next(50000) / 100m;
                            
                            var orderId = Guid.NewGuid();
                            // Build a message
                            var content = JsonConvert.SerializeObject(new { orderId = orderId.ToString(), amount = orderAmount });

                            // Send the message to simple-work-queue
                            channel.BasicPublish("orders", "", null, content.GetBytes());

                            Console.WriteLine("Order published with id '{0}'", orderId);

                            // Wait less than a second
                            Thread.Sleep(rnd.Next(1000));
                        }
                    }

                }
            }

        }
    }
}

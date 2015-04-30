using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RabbitDemo.Utilities;
using RabbitMQ.Client;
namespace RabbitDemo.HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost"
            };

            using (var conn = connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    Console.WriteLine("Declaring queue");
                    channel.QueueDeclare("hello-world-queue", true, false, false, null);

                    Console.WriteLine("Publishing message");
                    channel.BasicPublish("", "hello-world-queue", null, "Hello world".GetBytes());

                    Console.ReadLine();
                }
            }

        }

        
    }
}

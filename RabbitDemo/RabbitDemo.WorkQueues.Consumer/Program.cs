using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitDemo.Utilities;
using RabbitMQ.Client;

namespace RabbitDemo.WorkQueues.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Make sure the simple-work-queue exists
                channel.QueueDeclare("simple-work-queue", true, false, false, null);

                // Set prefetch to 1 (only fetch one message at a time)
                channel.BasicQos(0, 1, false);

                // Set up internal consumer queue. This will try to get as many messages as possible
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume("simple-work-queue", false, consumer);

                while (true)
                {
                    // Fetch a new message from the internal queue
                    var message = consumer.Queue.Dequeue();
                    var contents = message.Body.GetString();
                    
                    Console.WriteLine("Working on '{0}'", contents);
                    Thread.Sleep(1000);

                    channel.BasicAck(message.DeliveryTag, false);
                }
            }
        }
    }
}

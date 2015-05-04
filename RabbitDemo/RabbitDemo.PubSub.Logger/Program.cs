using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitDemo.Utilities;
using RabbitMQ.Client;

namespace RabbitDemo.PubSub.Logger
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
                    // Ensure there is an orders exchange
                    channel.ExchangeDeclare("orders", ExchangeType.Fanout, true);

                    // Create a queue for this consumer
                    var queue = channel.QueueDeclare("orders-logger", true, false, false, null);

                    // Bind it to the orders exchange
                    channel.QueueBind(queue.QueueName, "orders", "");

                    // Set up internal consumer queue. This will try to get as many messages as possible
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queue.QueueName, false, consumer);

                    Console.WriteLine("Listening for messages on '{0}'", queue.QueueName);

                    while (true)
                    {
                        // Fetch a new message from the internal queue
                        var message = consumer.Queue.Dequeue();
                        var contents = message.Body.GetString();

                        Console.WriteLine("Saving '{0}'", contents);

                        channel.BasicAck(message.DeliveryTag, false);
                    }

                }
            }

        }
    }
}

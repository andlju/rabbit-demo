using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitDemo.Utilities;
using RabbitMQ.Client;

namespace RabbitDemo.Routing.Logger
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
                    // Ensure the microservice-bus exchange has been created
                    channel.ExchangeDeclare("microservice-bus", ExchangeType.Topic, true);

                    // Create a queue for this handler
                    var queue = channel.QueueDeclare("logger", true, false, false, null);

                    // Bind it to the microservice-bus exchange, subscribe to everything
                    channel.QueueBind(queue.QueueName, "microservice-bus", "#");

                    // Set up internal consumer queue. This will try to get as many messages as possible
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queue.QueueName, true, consumer);

                    Console.WriteLine("Listening for messages on '{0}'", queue.QueueName);

                    while (true)
                    {
                        // Fetch a new message from the internal queue
                        var message = consumer.Queue.Dequeue();
                        var contents = message.Body.GetString();
                        Console.WriteLine(contents);
                    }

                }
            }
        }
    }
}

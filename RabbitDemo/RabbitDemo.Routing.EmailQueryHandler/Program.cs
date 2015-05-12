using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitDemo.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Content;

namespace RabbitDemo.Routing.EmailQueryHandler
{
    class Program
    {
        static Program()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        static void Main(string[] args)
        {
            var emailRegex = new Regex(@"^[^@]+@[^@]+\.[^@]+$");

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

                    var queueArgs = new Dictionary<string,object>();
                    queueArgs["x-dead-letter-exchange"] = "microservice-bus";
                    queueArgs["x-dead-letter-routing-key"] = "query.deadletter.email";
                    queueArgs["x-message-ttl"] = 5000;

                    // Create a queue for this handler
                    var queue = channel.QueueDeclare("email-query-handler", true, false, false, queueArgs);

                    // Bind it to the microservice-bus exchange, subscribe to the main query topic
                    channel.QueueBind(queue.QueueName, "microservice-bus", "query");

                    // Set up internal consumer queue. This will try to get as many messages as possible
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queue.QueueName, false, consumer);

                    Console.WriteLine("Listening for messages on '{0}'", queue.QueueName);

                    while (true)
                    {
                        // Fetch a new message from the internal queue
                        var message = consumer.Queue.Dequeue();
                        var contents = message.Body.GetString();
                        Console.WriteLine("Found message: ");
                        Console.WriteLine(contents);

                        dynamic msg = JsonConvert.DeserializeObject(contents);
                        string query = msg.query;
                        string requestId = msg.requestId;

                        if (query != null && emailRegex.IsMatch(query))
                        {
                            var contactId = FindContactIdByEmail(query);
                            if (contactId.HasValue)
                            {
                                // We found a match. Let's send a message back to the bus to let everyone know
                                // Since this is match by email address we are pretty confident that it's a good match
                                var contactIdMessage = new {requestId, contactId = contactId, confidence = 1};
                                var str = JsonConvert.SerializeObject(contactIdMessage);
                                channel.BasicPublish("microservice-bus", "query.contacts", null, str.GetBytes());
                            }
                        }

                        channel.BasicAck(message.DeliveryTag, false);
                    }

                }
            }

        }

        public static int? FindContactIdByEmail(string email)
        {
            if (email == "anders@ljusberg.se")
            {
                return 1;
            }
            return 0;
        }
    }
}

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

namespace RabbitDemo.Routing.TagQueryHandler
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

                    var queueArgs = new Dictionary<string, object>();
                    queueArgs["x-dead-letter-exchange"] = "microservice-bus";
                    queueArgs["x-dead-letter-routing-key"] = "query.deadletter.tag";
                    queueArgs["x-message-ttl"] = 5000;

                    // Create a queue for this handler
                    var queue = channel.QueueDeclare("tag-query-handler", true, false, false, queueArgs);

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

                        if (query != null)
                        {
                            var contactIds = FindContactIdsByTag(query);
                            foreach (var contactId in contactIds)
                            {
                                // We found a match. Let's send a message back to the bus to let everyone know
                                // Since this is only a match by tag address we set the confidence a bit lower
                                var contactIdMessage = new { requestId, contactId, confidence = 0.75 };
                                var str = JsonConvert.SerializeObject(contactIdMessage);
                                channel.BasicPublish("microservice-bus", "query.contacts", null, str.GetBytes());
                            }
                        }

                        channel.BasicAck(message.DeliveryTag, false);
                    }

                }
            }
        }

        private static IEnumerable<int> FindContactIdsByTag(string tag)
        {
            if (tag.Equals("swenug", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return 1;
                yield return 3;
                yield break;
            }
            if (tag.Equals("akka", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return 2;
                yield break;
            }
            if (tag.Equals("microsoft", StringComparison.InvariantCultureIgnoreCase))
            {
                yield return 4;
                yield break;
            }
        }
    }
}

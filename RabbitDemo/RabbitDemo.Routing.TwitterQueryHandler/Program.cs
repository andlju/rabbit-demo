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

namespace RabbitDemo.Routing.TwitterQueryHandler
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

        private static void Main(string[] args)
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
                    var queue = channel.QueueDeclare("twitter-query-handler", true, false, false, null);

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

                        if (query != null && query.StartsWith("@"))
                        {
                            var result = FindContactIdsByTwitter(query);
                            foreach (var item in result)
                            {
                                // We found a match. Let's send a message back to the bus to let everyone know
                                var contactIdMessage =
                                    new {requestId, contactId = item.Item1, confidence = item.Item2};
                                var str = JsonConvert.SerializeObject(contactIdMessage);
                                channel.BasicPublish("microservice-bus", "query.contacts", null, str.GetBytes());
                            }
                        }

                        channel.BasicAck(message.DeliveryTag, false);
                    }

                }
            }
        }

        private static List<Tuple<string,int>> contacts = new List<Tuple<string, int>>()
        {
            new Tuple<string,int>("@CodingInsomnia", 1),
            new Tuple<string,int>("@hcanber", 2),
            new Tuple<string,int>("@CeciliaSHARP", 3),
            new Tuple<string,int>("@buzzfrog", 4),
        };

        public static IEnumerable<Tuple<int, double>> FindContactIdsByTwitter(string twitter)
        {
            foreach (var contactInfo in contacts)
            {
                if (contactInfo.Item1.StartsWith(twitter))
                {
                    var confidence = twitter.Length * 1.0 / contactInfo.Item1.Length;
                    yield return new Tuple<int, double>(contactInfo.Item2, confidence);
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitDemo.Utilities;
using RabbitMQ.Client;

namespace RabbitDemo.Routing.ContactIdQueryHandler
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

                    // Create a queue for this handler
                    var queue = channel.QueueDeclare("contactid-query-handler", true, false, false, null);

                    // Bind it to the microservice-bus exchange, subscribe to the contacts query topic
                    channel.QueueBind(queue.QueueName, "microservice-bus", "query.contacts");

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

                        string requestId = msg.requestId;
                        int contactId = msg.contactId;
                        double confidence = msg.confidence;

                        var contact = GetContactById(contactId);
                        if (contact != null)
                        {
                            // Found a contact with this Id. Let's send it to anyone listening.
                            var contactMessage = new {requestId, confidence, contact};
                            var str = JsonConvert.SerializeObject(contactMessage);
                            channel.BasicPublish("microservice-bus", "response.contact", null, str.GetBytes());
                        }

                        channel.BasicAck(message.DeliveryTag, false);
                    }
                }
            }
        }

        private static Contact GetContactById(int contactId)
        {
            switch (contactId)
            {
                case 1:
                    return new Contact()
                    {
                        ContactId = contactId,
                        FirstName = "Anders",
                        LastName = "Ljusberg",
                        Twitter = "@CodingInsomnia"
                    };
                case 2:
                    return new Contact()
                    {
                        ContactId = contactId,
                        FirstName = "Håkan",
                        LastName = "Canberger",
                        Twitter = "@hcanber"
                    };
                case 3:
                    return new Contact()
                    {
                        ContactId = contactId,
                        FirstName = "Cecilia",
                        LastName = "Wirén",
                        Twitter = "@CeciliaSHARP"
                    };
                case 4:
                    return new Contact()
                    {
                        ContactId = contactId,
                        FirstName = "Dag",
                        LastName = "König",
                        Twitter = "@Buzzfrog"
                    };
            }
            return null;
        }
    }

    class Contact
    {
        public int ContactId;
        public string FirstName;
        public string LastName;
        public string Twitter;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitDemo.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace RabbitDemo.WorkQueues.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Make sure the simple-work-queue exists
                channel.QueueDeclare("simple-work-queue", true, false, false, null);
                
                Console.Write("Give your producer a name: ");
                var producerName = Console.ReadLine();

                while (true)
                {
                    Console.Write("Number of messages to send: ");
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                        break;
                    
                    int numberOfMessages;
                    int.TryParse(input, out numberOfMessages);
                    
                    for (int i = 0; i < numberOfMessages; i++)
                    {
                        // Build a message
                        var content = string.Format("{0} message number {1}", producerName, i);

                        // Send the message to simple-work-queue
                        channel.BasicPublish("", "simple-work-queue", null, content.GetBytes());
                    }

                    Console.WriteLine("\n{0} messages published", numberOfMessages);
                }
            }
        }
    }
}

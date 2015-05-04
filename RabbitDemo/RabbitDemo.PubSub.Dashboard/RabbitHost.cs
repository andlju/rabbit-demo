using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RabbitDemo.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitDemo.PubSub.Dashboard
{
    public class RabbitHost
    {
        readonly ConnectionFactory _connectionFactory;
        private bool _running;

        public RabbitHost()
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
        }

        public void Run()
        {
            _running = true;
            using (var conn = _connectionFactory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    // Ensure there is an orders exchange
                    channel.ExchangeDeclare("orders", ExchangeType.Fanout, true);

                    // Create a temporary queue for this consumer
                    var queue = channel.QueueDeclare();

                    // Bind it to the orders exchange
                    channel.QueueBind(queue.QueueName, "orders", "");

                    // Set up internal consumer queue. This will try to get as many messages as possible
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queue.QueueName, false, consumer);

                    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<OrderInfoHub>();

                    while (_running)
                    {
                        // Fetch a new message from the internal queue. Only wait 1 second, then retry
                        BasicDeliverEventArgs message;
                        if (!consumer.Queue.Dequeue(1000, out message))
                            continue;

                        var contents = message.Body.GetString();
                        
                        // Get the message contents as a dynamic object
                        dynamic order = JsonConvert.DeserializeObject(contents);
                        // Send to all connected SignalR clients
                        hubContext.Clients.All.orderPlaced((string) order.orderId, (decimal) order.amount);

                        channel.BasicAck(message.DeliveryTag, false);
                    }

                }
            }
        }

        public void Stop()
        {
            // Stop the message loop
            _running = false;
        }
    }
}
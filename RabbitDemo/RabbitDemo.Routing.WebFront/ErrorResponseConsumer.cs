using System;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RabbitDemo.Routing.WebFront.Controllers;
using RabbitDemo.Utilities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitDemo.Routing.WebFront
{
    public class ErrorResponseConsumer
    {
        readonly ConnectionFactory _connectionFactory;
        private bool _running;

        public ErrorResponseConsumer()
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
                    // Ensure there is a dead-letter exchange
                    channel.ExchangeDeclare("microservice-bus", ExchangeType.Topic, true);

                    // Create a temporary queue for this consumer
                    var queue = channel.QueueDeclare();

                    // Bind it to the microservice-bus exchange, subscribing to all unprocessed queries
                    channel.QueueBind(queue.QueueName, "microservice-bus", "query.deadletter.*");

                    // Set up internal consumer queue. This will try to get as many messages as possible
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queue.QueueName, false, consumer);

                    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SearchHub>();

                    while (_running)
                    {
                        // Fetch a new message from the internal queue. Only wait 1 second, then retry
                        BasicDeliverEventArgs message;
                        if (!consumer.Queue.Dequeue(1000, out message))
                            continue;

                        var contents = message.Body.GetString();
                        var routingkey = message.RoutingKey;

                        // Get the message contents as a dynamic object to extract the requestId
                        dynamic responseMsg = JsonConvert.DeserializeObject(contents);
                        responseMsg.routingKey = routingkey;
                        responseMsg.handlerName = routingkey.Substring(routingkey.LastIndexOf(".", StringComparison.InvariantCulture) + 1);

                        // Send to the clients that have subscribed to this requestId
                        hubContext.Clients.All.queryTimedOut(responseMsg);

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
using System;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using RabbitDemo.Utilities;
using RabbitMQ.Client;

namespace RabbitDemo.Routing.WebFront.Controllers
{
    public class SearchHub : Hub
    {
        private readonly IModel _channel;

        public SearchHub(IModel channel)
        {
            _channel = channel;
        }

        public void Search(string query)
        {
            // Create a requestId to correlate the flow
            var requestId = Guid.NewGuid().ToString();
            
            // Subscribe the client to responses with this requestId
            Groups.Add(Context.ConnectionId, requestId);

            // Build the query message
            var queryMessage = new {requestId, query};
            var msg = JsonConvert.SerializeObject(queryMessage);
            
            // Send this query to the bus
            _channel.BasicPublish("microservice-bus", "query", null, msg.GetBytes());
            
        }
    }
}
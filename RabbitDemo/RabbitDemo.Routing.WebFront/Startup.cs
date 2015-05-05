using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using RabbitDemo.Routing.WebFront.Controllers;
using RabbitMQ.Client;

[assembly: OwinStartupAttribute(typeof(RabbitDemo.Routing.WebFront.Startup))]
namespace RabbitDemo.Routing.WebFront
{

    public partial class Startup
    {
        private readonly IModel _channel;

        public Startup()
        {
            var connectionFactory  = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            var conn = connectionFactory.CreateConnection();
            _channel = conn.CreateModel();
            
            // Ensure the microservice-bus exchange has been created
            _channel.ExchangeDeclare("microservice-bus", ExchangeType.Topic, true);
        }

        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.Register(typeof(SearchHub), () => new SearchHub(_channel));
            app.MapSignalR();
        }
    }
}

using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(RabbitDemo.PubSub.Dashboard.Startup))]

namespace RabbitDemo.PubSub.Dashboard
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}

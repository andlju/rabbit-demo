using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace RabbitDemo.PubSub.Dashboard
{
    public class Global : System.Web.HttpApplication
    {
        RabbitHost _rabbitHost = new RabbitHost();

        protected void Application_Start(object sender, EventArgs e)
        {
            var hostThread = new Thread(_rabbitHost.Run);
            hostThread.Start();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            _rabbitHost.Stop();
        }
    }
}
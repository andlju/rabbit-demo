using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RabbitDemo.Routing.WebFront
{
    public class MvcApplication : System.Web.HttpApplication
    {
        RabbitHost _rabbitHost = new RabbitHost();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var hostThread = new Thread(_rabbitHost.Run);
            hostThread.Start();

        }
        protected void Application_End(object sender, EventArgs e)
        {
            _rabbitHost.Stop();
        }
    }
}

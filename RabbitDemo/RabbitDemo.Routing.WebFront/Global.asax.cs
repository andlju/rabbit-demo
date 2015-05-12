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
        QueryResponseConsumer _queryResponseConsumer = new QueryResponseConsumer();
        ErrorResponseConsumer _errorResponseConsumer = new ErrorResponseConsumer();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var queryResponseThread = new Thread(_queryResponseConsumer.Run);
            queryResponseThread.Start();
            var errorResponseThread = new Thread(_errorResponseConsumer.Run);
            errorResponseThread.Start();

        }
        protected void Application_End(object sender, EventArgs e)
        {
            _queryResponseConsumer.Stop();
            _errorResponseConsumer.Stop();
        }
    }
}

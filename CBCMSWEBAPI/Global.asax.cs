using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CBCMSWEBAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var application = sender as HttpApplication;
            if (application != null && application.Context != null)
            {
                application.Context.Response.Headers.Remove("Server");
                application.Context.Response.Headers.Remove("X-AspNet-Version");
                application.Context.Response.Headers.Remove("X-AspNetMvc-Version");
                application.Context.Response.Headers.Remove("X-AspNetMvc-Version");
            }
        }
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        //{
        //    HttpContext.Current.Response.Headers.Remove("X-Powered-By");
        //    HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
        //    HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
        //    HttpContext.Current.Response.Headers.Remove("Server");
        //    Response.Headers.Remove("Server");
        //    Response.AddHeader("Server", "Reliance Generl Insurance Company Ltd.");
        //}

        //protected void Application_PreSendRequestHeaders()
        //{
        //    HttpContext.Current.Response.Headers.Remove("X-Powered-By");
        //    HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
        //    HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
        //    HttpContext.Current.Response.Headers.Remove("Server");
        //    Response.AddHeader("Server", "Reliance Generl Insurance Company Ltd.");
        //}

        //protected void Application_Start()
        //{
        //    AreaRegistration.RegisterAllAreas();
        //    GlobalConfiguration.Configure(WebApiConfig.Register);
        //    FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
        //    RouteConfig.RegisterRoutes(RouteTable.Routes);
        //    BundleConfig.RegisterBundles(BundleTable.Bundles);
        //    MvcHandler.DisableMvcResponseHeader = true;
        //}
    }
}

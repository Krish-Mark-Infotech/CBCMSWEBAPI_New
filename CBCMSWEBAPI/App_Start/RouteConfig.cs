using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CBCMSWEBAPI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                //url: "{controller}/{action}/{id}",
                //url: "api/{controller}/{action}/{jsonInput}",
                url : "api/{controller}/{action}/{key}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "FailWhale",
               url: "FailWhale/{action}/{id}",
               defaults: new { controller = "ErrorPage", action = "FailWhale", id = UrlParameter.Optional }
           );
        }
    }
}

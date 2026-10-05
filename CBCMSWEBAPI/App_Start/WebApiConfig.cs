using CBCMSWEBAPI.Handlers;
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Extensions.Compression.Core.Compressors;
using System.Web.Http;

namespace CBCMSWEBAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new HttpMethodValidationHandler());
            // Web API configuration and services
            config.EnableCors();
            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                //routeTemplate: "api/{controller}/{id}",
                routeTemplate: "api/{controller}/{action}/{jsonInput}",
                defaults: new { id = RouteParameter.Optional }
            );
            GlobalConfiguration.Configuration.MessageHandlers.Insert(0,
               new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));
        }
    }
}

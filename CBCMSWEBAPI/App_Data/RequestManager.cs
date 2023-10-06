using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for RequestManager
/// </summary>
public class RequestManager
{
    public RequestManager()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static void Application_BeginRequest(Object source, EventArgs e)

    {

        HttpApplication app = (HttpApplication)source;

        HttpContext context = app.Context;

        // Attempt to peform first request initialization

        FirstRequestInitialization.Initialize(context);

    }

    public static void Validate(HttpContext context)
    {
        if (context.Application["Blocklist"] == null) context.Application.Add("Blocklist", new Dictionary<string, DateTime>());
        Dictionary<string, DateTime>
     blocklist = context.Application["Blocklist"] as Dictionary<string, DateTime>;
        if (blocklist.ContainsKey(context.Request.UserHostAddress))
        {
            DateTime lastRequest = blocklist[context.Request.UserHostAddress];
            if (DateTime.Now.Subtract(lastRequest).TotalMilliseconds <
            1000)
            {
                // End request
                context.Response.Write(string.Format("You'll have to wait for {0} milliseconds until next request", 1000 - DateTime.Now.Subtract(lastRequest).TotalMilliseconds));
                context.Response.End();
            }
            else
            {
                blocklist[context.Request.UserHostAddress] = DateTime.Now;
            }
        }
        else
        {
            blocklist.Add(context.Request.UserHostAddress, DateTime.Now);
        }
    }

    public class FirstRequestInitialization

    {

        private static bool s_InitializedAlready = false;

        private static Object s_lock = new Object();

        // Initialize only on the first request

        public static void Initialize(HttpContext context)

        {

            if (s_InitializedAlready)

            {

                return;

            }

            lock (s_lock)

            {

                if (s_InitializedAlready)

                {

                    return;

                }

                // Perform first-request initialization here …

                s_InitializedAlready = true;

            }

        }

    }
}
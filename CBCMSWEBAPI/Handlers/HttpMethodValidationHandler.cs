using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

namespace CBCMSWEBAPI.Handlers
{
    public class HttpMethodValidationHandler : DelegatingHandler
    {
        private static readonly HashSet<string> AllowedMethods = new HashSet<string> { "GET", "POST", "PUT", "DELETE" };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!AllowedMethods.Contains(request.Method.Method.ToUpperInvariant()))
            {
                var response = request.CreateResponse(HttpStatusCode.MethodNotAllowed);
                response.Content = new StringContent("HTTP method not allowed.");

                response.Headers.Server.Clear();
                return Task.FromResult(response);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
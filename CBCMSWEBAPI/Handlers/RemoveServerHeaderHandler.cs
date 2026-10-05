using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

public class RemoveServerHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        response.Headers.Server.Clear(); // Removes Server header

        return response;
    }
}
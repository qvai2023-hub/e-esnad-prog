using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace EtaskMinstry.Api.Configuration
{
    /// <summary>
    /// Adds CORS response headers to every /api/v1/ response. We don't depend
    /// on Microsoft.AspNet.WebApi.Cors (not installed) — a small message handler
    /// keeps the surface area minimal and avoids package additions.
    ///
    /// Defaults: origin "*" (mobile apps don't enforce CORS, but devtools and
    /// PWA previews do). Override the allowed origin via Web.config key
    /// "ApiCorsAllowedOrigin" if you need to lock it down.
    /// </summary>
    public static class ApiCorsConfig
    {
        public static void Install(HttpConfiguration config)
        {
            config.MessageHandlers.Add(new CorsHandler());
        }

        private class CorsHandler : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                bool isPreflight = request.Method == HttpMethod.Options
                    && request.Headers.Contains("Origin");

                if (isPreflight)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    AddHeaders(request, response);
                    var tcs = new TaskCompletionSource<HttpResponseMessage>();
                    tcs.SetResult(response);
                    return tcs.Task;
                }

                return base.SendAsync(request, cancellationToken).ContinueWith(t =>
                {
                    var response = t.Result;
                    AddHeaders(request, response);
                    return response;
                }, cancellationToken);
            }

            private static void AddHeaders(HttpRequestMessage request, HttpResponseMessage response)
            {
                string allowedOrigin = System.Configuration.ConfigurationManager
                    .AppSettings["ApiCorsAllowedOrigin"] ?? "*";

                string origin = allowedOrigin == "*"
                    ? "*"
                    : (request.Headers.Contains("Origin") ? string.Join(",", request.Headers.GetValues("Origin")) : allowedOrigin);

                if (!response.Headers.Contains("Access-Control-Allow-Origin"))
                    response.Headers.Add("Access-Control-Allow-Origin", origin);

                if (!response.Headers.Contains("Access-Control-Allow-Methods"))
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

                if (!response.Headers.Contains("Access-Control-Allow-Headers"))
                    response.Headers.Add("Access-Control-Allow-Headers", "Authorization, Content-Type, Accept");

                if (!response.Headers.Contains("Access-Control-Max-Age"))
                    response.Headers.Add("Access-Control-Max-Age", "600");
            }
        }
    }
}

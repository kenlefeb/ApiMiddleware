using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kenlefeb.Api.Middleware.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace Kenlefeb.Api.Middleware
{
    public class RequestLogging
    {
        private readonly TelemetryClient _telemetry;
        readonly RequestDelegate _next;

        public RequestLogging(RequestDelegate next, TelemetryClient telemetry)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _telemetry = telemetry;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));


            httpContext.Response.OnCompleted(PublishRequestResponse, httpContext);
            await _next(httpContext).ConfigureAwait(true);

        }

        private async Task PublishRequestResponse(object parameter)
        {
            var httpContext = parameter as HttpContext;
            if (httpContext == null)
                throw new ArgumentNullException(nameof(parameter));

            var properties = new Dictionary<string, string>
                             {
                                 { "Request", await CollectRequest(httpContext).ConfigureAwait(true) },
                                 { "Response", await CollectResponse(httpContext).ConfigureAwait(true) },
                             };
            var metrics = CollectMetrics(); //(httpContext);
            this._telemetry.TrackEvent("HTTP Request", properties, metrics);
        }

        private static IDictionary<string, double> CollectMetrics() //(HttpContext httpContext)
        {
            return new Dictionary<string, double>{ };
        }

        private static async Task<string> CollectResponse(HttpContext httpContext)
        {
            var response = new Response(httpContext);
            try
            {
                using (var reader = new StreamReader(httpContext.Response.Body))
                    response.Content.Body = await reader.ReadToEndAsync().ConfigureAwait(true);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore any errors reading the request body
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return System.Text.Json.JsonSerializer.Serialize(response);
        }

        private static async Task<string> CollectRequest(HttpContext httpContext)
        {
            var request = new Request(httpContext);

            try
            {
                using (var reader = new StreamReader(httpContext.Request.Body))
                    request.Content.Body = await reader.ReadToEndAsync().ConfigureAwait(true);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore any errors reading the request body
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return System.Text.Json.JsonSerializer.Serialize(request);
        }

    }
}

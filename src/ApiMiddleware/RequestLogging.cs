using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kenlefeb.Api.Middleware.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http.Internal;

namespace Kenlefeb.Api.Middleware
{
    public class RequestLogging
    {
        private readonly TelemetryClient _telemetry;
        readonly RequestDelegate _next;
        private string _response;
        private string _request;

        public RequestLogging(RequestDelegate next, TelemetryClient telemetry)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _telemetry = telemetry;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var originals = new
                            {
                                Response = httpContext.Response.Body,
                            };

            _request = SaveRequestBody(httpContext.Request).Result;

            using (var response = new MemoryStream())
            {
                httpContext.Response.Body = response;
                httpContext.Response.OnCompleted(PublishRequestResponse, httpContext);
                await _next(httpContext).ConfigureAwait(true);
                _response = await SaveResponseBody(httpContext.Response.Body).ConfigureAwait(true);
                await response.CopyToAsync(originals.Response).ConfigureAwait(true);
            }
        }

        private static async Task<string> SaveRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            var body = default(string);
            try
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var reader = new StreamReader(request.Body);
#pragma warning restore CA2000 // Dispose objects before losing scope
                body = await reader.ReadToEndAsync().ConfigureAwait(true);
                request.Body.Seek(0, SeekOrigin.Begin);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore any errors reading the request body
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return body;
        }

        private static async Task<string> SaveResponseBody(Stream stream)
        {
            var body = default(string);
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
#pragma warning disable CA2000 // Dispose objects before losing scope
                body = await new StreamReader(stream).ReadToEndAsync().ConfigureAwait(true);
#pragma warning restore CA2000 // Dispose objects before losing scope
                stream.Seek(0, SeekOrigin.Begin);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore any errors reading the request body
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return body;
        }

        private Task PublishRequestResponse(object parameter)
        {
            var httpContext = parameter as HttpContext;
            if (httpContext == null)
                throw new ArgumentNullException(nameof(parameter));

            var properties = new Dictionary<string, string>
                             {
                                 { "Request", CollectRequest(httpContext) },
                                 { "Response", CollectResponse(httpContext) },
                             };
            var metrics = CollectMetrics(); //(httpContext);
            this._telemetry.TrackEvent("HTTP Request", properties, metrics);
            return Task.CompletedTask;
        }

        private static IDictionary<string, double> CollectMetrics() //(HttpContext httpContext)
        {
            return new Dictionary<string, double> { };
        }

        private string CollectResponse(HttpContext httpContext)
        {
            var response = new Response(httpContext);
            response.Content.Body = _response;

            return System.Text.Json.JsonSerializer.Serialize(response);
        }

        private string CollectRequest(HttpContext httpContext)
        {
            var request = new Request(httpContext);
            request.Content.Body = _request;

            return System.Text.Json.JsonSerializer.Serialize(request);
        }

    }
}

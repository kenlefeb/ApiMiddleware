using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Kenlefeb.Api.Middleware
{
    public class RequestLogging
    {
        private readonly TelemetryClient _telemetry;
        readonly RequestDelegate _next;

        public RequestLogging(RequestDelegate next, TelemetryClient telemetry)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            using (var operation = _telemetry.StartOperation<RequestTelemetry>("BPM Middleware Request"))
            {
                if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

                var before = httpContext.Response.Body;
                var body = default(string);

                var start = Stopwatch.GetTimestamp();
                var elapsed = default(double);
                try
                {
                    using (var memory = new MemoryStream())
                    {
                        httpContext.Response.Body = memory;

                        await _next(httpContext);
                        elapsed = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                        memory.Position = 0;
                        body = new StreamReader(memory).ReadToEnd();
                        memory.Position = 0;

                        await memory.CopyToAsync(before);
                    }

                    var statusCode = httpContext.Response?.StatusCode;
                    var level = statusCode > 499 ? LogLevel.Error : LogLevel.Information;

                    var request = httpContext.Request;
                    var response = httpContext.Response;

                    var properties = CollectProperties(httpContext);
                    if (!string.IsNullOrEmpty(body))
                        properties.Add("Response", body);

                    var metrics = new Dictionary<string, double>
                                  {
                                      {"Duration", elapsed},
                                  };
                    this._telemetry.TrackEvent("HTTP Request", properties, metrics);
                }
                // Never caught, because `LogException()` returns false.
                catch (Exception ex)
                    when (LogException(httpContext, GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex))
                { }
                finally
                {
                    httpContext.Response.Body = before;
                }
            }
        }

        private static Dictionary<string, string> CollectProperties(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            var properties = new Dictionary<string, string>();

            foreach (var header in request.Headers)
                properties.Add($"Request: {header.Key}", $"{header.Value}");

            foreach (var header in response.Headers)
                properties.Add($"Response: {header.Key}", $"{header.Value}");

            properties.Add("Method", request.Method);
            properties.Add("Path", GetPath(httpContext));
            properties.Add("Host", $"{request.Host}");
            properties.Add("Protocol", request.Protocol);
            properties.Add("StatusCode", $"{(int)response.StatusCode} {response.StatusCode}");

            var body = GetRequestBody(request);
            if (!string.IsNullOrEmpty(body))
                properties.Add("Request", body);

            if (request.Query.Any())
            {
                foreach (var parameter in request.Query)
                {
                    properties.Add($"Query: {parameter.Key}", parameter.Value);
                }
            }

            return properties;
        }

        private static string GetRequestBody(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                var before = request.Body.Position;
                request.Body.Position = 0;
                using (var reader = new StreamReader(request.Body))
                {
                    var body = reader.ReadToEnd();
                    request.Body.Position = before;
                    return body;
                }
            }
            catch
            {
                return null;
            }
        }

        bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
        {
            var properties = CollectProperties(httpContext);
            this._telemetry.TrackException(ex, properties);
            return false;
        }

        static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }

        static string GetPath(HttpContext httpContext)
        {
            return httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
        }
    }
}

﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Fody;
using Kenlefeb.Api.Middleware.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Kenlefeb.Api.Middleware
{
    /// <summary>
    /// Middleware to log all HTTP Requests and Responses.
    /// </summary>
    [ConfigureAwait(false)]
    public class RequestLogging
    {
        /// <summary>
        /// The next
        /// </summary>
        /// <autogeneratedoc />
        private readonly RequestDelegate _next;
        /// <summary>
        /// The telemetry
        /// </summary>
        /// <autogeneratedoc />
        private readonly TelemetryClient _telemetry;
        /// <summary>
        /// The options
        /// </summary>
        /// <autogeneratedoc />
        private readonly RequestLoggingOptions _options;
        /// <summary>
        /// The request
        /// </summary>
        /// <autogeneratedoc />
        private string          _request = string.Empty;
        /// <summary>
        /// The response
        /// </summary>
        /// <autogeneratedoc />
        private string          _response = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogging" /> class.
        /// </summary>
        /// <param name="next">The next RequestDelegate in the pipeline.</param>
        /// <param name="telemetry">The Application Insights telemetry client.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">next</exception>
        public RequestLogging(RequestDelegate next, TelemetryClient telemetry, RequestLoggingOptions options)
        {
            _next      = next ?? throw new ArgumentNullException(nameof(next));
            _telemetry = telemetry;
            _options = options;
        }

        /// <summary>
        /// When this is invoked by the previous RequestDelegate in the pipeline, the Request body is
        /// captured and the rest of the pipeline is continued. When the Response comes back down the
        /// pipeline, the Response body is captured and all the Request and Response metadata is
        /// published to Application Insights.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <exception cref="ArgumentNullException">httpContext</exception>
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var originals = new {Response = httpContext.Response.Body};

            _request = SaveRequestBody(httpContext.Request)
                .Result;

            httpContext.Response.Body = new MemoryStream();

            httpContext.Response.OnCompleted(PublishRequestResponse, httpContext);

            await _next(httpContext);

            _response = await SaveResponseBody(httpContext.Response.Body);

            await httpContext.Response.Body.CopyToAsync(originals.Response);
        }

        /// <summary>
        /// Saves the request body.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        /// <autogeneratedoc />
        private static async Task<string> SaveRequestBody(HttpRequest request)
        {
            request.EnableBuffering();
            var body = default(string);
            try
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                var reader = new StreamReader(request.Body);
#pragma warning restore CA2000 // Dispose objects before losing scope
                body = await reader.ReadToEndAsync();
                request.Body.Seek(0, SeekOrigin.Begin);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore any errors reading the request body
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return body ?? string.Empty;
        }

        /// <summary>
        /// Saves the response body.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        /// <autogeneratedoc />
        private static async Task<string> SaveResponseBody(Stream stream)
        {
            var body = default(string);
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
#pragma warning disable CA2000 // Dispose objects before losing scope
                body = await new StreamReader(stream).ReadToEndAsync();
#pragma warning restore CA2000 // Dispose objects before losing scope
                stream.Seek(0, SeekOrigin.Begin);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
            {
                // Ignore any errors reading the request body
            }
#pragma warning restore CA1031 // Do not catch general exception types
            return body ?? string.Empty;
        }

        /// <summary>
        /// Publishes the request response.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>Task.</returns>
        /// <exception cref="ArgumentNullException">parameter</exception>
        /// <autogeneratedoc />
        private Task PublishRequestResponse(object parameter)
        {
            var httpContext = parameter as HttpContext;
            if (httpContext == null)
                throw new ArgumentNullException(nameof(parameter));

            var properties = new Dictionary<string, string>
                             {
                                 {"Request", CollectRequest(httpContext)},
                                 {"Response", CollectResponse(httpContext)}
                             };
            var metrics = CollectMetrics(); //(httpContext);
            _telemetry.TrackEvent(ComposeEventName(), properties, metrics);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Composes the name of the event.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <autogeneratedoc />
        private string ComposeEventName()
        {
            if (!string.IsNullOrEmpty(_options.EventName))
                return _options.EventName;
            return "HTTP Request/Response";
        }

        /// <summary>
        /// Collects the metrics.
        /// </summary>
        /// <returns>IDictionary&lt;System.String, System.Double&gt;.</returns>
        /// <autogeneratedoc />
        private static IDictionary<string, double> CollectMetrics() //(HttpContext httpContext)
        {
            return new Dictionary<string, double>();
        }

        /// <summary>
        /// Collects the response.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>System.String.</returns>
        /// <autogeneratedoc />
        private string CollectResponse(HttpContext httpContext)
        {
            var response = new Response(httpContext);
            response.Content.Body = _response;

            return JsonSerializer.Serialize(response);
        }

        /// <summary>
        /// Collects the request.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>System.String.</returns>
        /// <autogeneratedoc />
        private string CollectRequest(HttpContext httpContext)
        {
            var request = new Request(httpContext);
            request.Content.Body = _request;

            return JsonSerializer.Serialize(request);
        }
    }
}

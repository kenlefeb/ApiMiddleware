using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kenlefeb.Api.Middleware.Models
{
    public class Request
    {
        public Request(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            Headers = httpContext.Request.Headers.ToDictionary(h => h.Key, h => $"{h.Value}");
            Method = httpContext.Request.Method;
            Host = $"{httpContext.Request.Host}";
            Protocol = httpContext.Request.Protocol;
            Query = httpContext.Request.Query.ToDictionary(q => q.Key, q => $"{q.Value}");
            Content = new Content
            {
                Type = httpContext.Request.ContentType,
                Length = httpContext.Request.ContentLength,
            };
        }

        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public string Method { get; set; }
        public string Host { get; set; }
        public string Protocol { get; set; }
        public Dictionary<string, string> Query { get; } = new Dictionary<string, string>();
        public Content Content { get; set; }
    }

    public class Response
    {
        public Response()
        {
        }

        public Response(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            StatusCode = httpContext.Response.StatusCode;
            Headers = httpContext.Response.Headers.ToDictionary(h => h.Key, h => $"{h.Value}");
            Content = new Content
            {
                Type = httpContext.Response.ContentType,
                Length = httpContext.Response.ContentLength
            };
        }

        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public Content Content { get; set; }
    }

    public class Content
    {
        public string Type { get; set; }
        public long? Length { get; set; }
        public string Body { get; set; }
    }
}
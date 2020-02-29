using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Kenlefeb.Api.Middleware.Models
{
    public class Response
    {
        public Response() { }

        public Response(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            StatusCode = httpContext.Response.StatusCode;
            Headers    = httpContext.Response.Headers.ToDictionary(h => h.Key, h => $"{h.Value}");
            Content = new Content
                      {
                          Type   = httpContext.Response.ContentType,
                          Length = httpContext.Response.ContentLength
                      };
        }

        public int                        StatusCode { get; set; }
        public Dictionary<string, string> Headers    { get; } = new Dictionary<string, string>();
        public Content                    Content    { get; set; }
    }
}

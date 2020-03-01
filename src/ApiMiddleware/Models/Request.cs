/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Kenlefeb.Api.Middleware.Models
{
    /// <summary>
    /// An HTTP Request Message.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Request" /> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <exception cref="ArgumentNullException">httpContext</exception>
        public Request(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            Headers  = httpContext.Request.Headers.ToDictionary(h => h.Key, h => $"{h.Value}");
            Method   = httpContext.Request.Method;
            Host     = $"{httpContext.Request.Host}";
            Protocol = httpContext.Request.Protocol;
            Query    = httpContext.Request.Query.ToDictionary(q => q.Key, q => $"{q.Value}");
            Content = new Content
                      {
                          Type   = httpContext.Request.ContentType,
                          Length = httpContext.Request.ContentLength
                      };
        }

        /// <summary>
        /// Gets the HTTP Headers.
        /// </summary>
        /// <value>The headers.</value>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the HTTP Method (<i>e.g.</i>, GET, POST, PUT, etc.) used to make this request.
        /// </summary>
        /// <value>The method.</value>
        public string Method { get; }

        /// <summary>
        /// Gets the Host header.
        /// </summary>
        /// <value>The host.</value>
        public string Host { get; }

        /// <summary>
        /// Gets the HTTP protocol used for this message.
        /// </summary>
        /// <value>The protocol.</value>
        public string Protocol { get; }

        /// <summary>
        /// Gets the QueryString parameters, if any.
        /// </summary>
        /// <value>The query.</value>
        public Dictionary<string, string> Query { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        public Content Content { get; }
    }
}

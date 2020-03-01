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
    /// An HTTP Response message.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Response" /> class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <exception cref="ArgumentNullException">httpContext</exception>
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

        /// <summary>
        /// Gets the status code.
        /// </summary>
        /// <value>The status code.</value>
        public int StatusCode { get; }

        /// <summary>
        /// Gets the HTTP Headers.
        /// </summary>
        /// <value>The headers.</value>
        public Dictionary<string, string> Headers { get; }

        /// <summary>
        /// Gets the content.
        /// </summary>
        /// <value>The content.</value>
        public Content Content { get; }
    }
}

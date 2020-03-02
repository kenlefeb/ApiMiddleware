﻿using System;
using Microsoft.AspNetCore.Builder;

namespace Kenlefeb.Api.Middleware
{
    /// <summary>
    /// Class ExtensionMethods.
    /// </summary>
    /// <autogeneratedoc />
    public static class ExtensionMethods
    {
        /// <summary>
        /// Uses the request logging.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configure">The configure.</param>
        /// <returns>IApplicationBuilder.</returns>
        /// <autogeneratedoc />
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, Action<RequestLoggingOptions> configure = null)
        {
            var options = new RequestLoggingOptions();
            configure?.Invoke(options);
            builder.UseMiddleware<RequestLogging>(options);
            return builder;
        }

    }
}

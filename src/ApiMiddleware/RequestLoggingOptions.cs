﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at https://mozilla.org/MPL/2.0/.
//  */

namespace Kenlefeb.Api.Middleware
{
    /// <summary>
    /// Options for the RequestLogging middleware.
    /// </summary>
    public class RequestLoggingOptions
    {
        /// <summary>
        /// Gets or sets the name to use for logging an HTTP request in Application Insights.
        /// </summary>
        /// <value>The name of the event.</value>
        public string? EventName { get; set; }

        /// <summary>
        /// Gets or sets the capture.
        /// </summary>
        /// <value>The capture.</value>
        /// <autogeneratedoc />
        public CaptureOptions Capture { get; set; } = new CaptureOptions { };
    }
}


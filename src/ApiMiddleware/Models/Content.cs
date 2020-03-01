/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
namespace Kenlefeb.Api.Middleware.Models
{
    /// <summary>
    /// The content of an HTTP request or response.
    /// </summary>
    public class Content
    {
        /// <summary>
        /// Gets or sets the Content-Type of this message.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Content-Length of this message.
        /// </summary>
        /// <value>The length.</value>
        /// <remarks>This is the value of the Content-Length header,
        /// and does not necessarily reflect the actual length of the Body.</remarks>
        public long? Length { get; set; }

        /// <summary>
        /// Gets or sets the actual string contents of the message body.
        /// </summary>
        /// <value>The body.</value>
        public string Body { get; set; } = string.Empty;
    }
}

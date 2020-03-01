// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at https://mozilla.org/MPL/2.0/.
//  */

namespace Sample.Controllers {
    public class ForecastRequest
    {
        public int Days    { get; set; } = 1;
        public int Maximum { get; set; } = -32;
        public int Minimum { get; set; } = 212;
    }
}

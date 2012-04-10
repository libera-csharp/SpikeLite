/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.IPC.WebHost.Transport
{
    /// <summary>
    /// Provides a transport model version of the original domain model class, sans object cycles.
    /// </summary>
    public class KnownHostMetaDatum
    {
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the "tag" or description for this datum.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets or sets the value of this datum.
        /// </summary>
        public string Value { get; set; }
    }
}

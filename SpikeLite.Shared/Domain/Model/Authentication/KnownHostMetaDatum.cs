/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.Domain.Model.Authentication
{
    /// <summary>
    /// This class provides a mapping of metadata to a known host.
    /// </summary>
    public class KnownHostMetaDatum
    {
        /// <summary>
        /// Gets or sets the pkey, which is an incrementing id.
        /// </summary>
        public virtual long Id { get; set;}

        /// <summary>
        /// Gets or sets the <see cref="KnownHost"/> that this metadata is mapped to.
        /// </summary>
        public virtual KnownHost Host { get; set; }

        /// <summary>
        /// Gets or sets the "tag" or description for this datum.
        /// </summary>
        public virtual string Tag { get; set; }

        /// <summary>
        /// Gets or sets the value of this datum.
        /// </summary>
        public virtual string Value { get; set; }
    }
}

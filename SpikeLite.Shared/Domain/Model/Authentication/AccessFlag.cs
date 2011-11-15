/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

namespace SpikeLite.Domain.Model.Authentication
{
    /// <summary>
    /// Provides an "access" flag - a fine-grained mechanism for determining access to a resource. 
    /// </summary>
    public class AccessFlag
    {
        /// <summary>
        /// This should be generated and set by our ORM. This is our PKey, non-naturalized for her pleasure.
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Holds the actual "flag" as it were - where a flag can be any combination of characters really. As long as they're unique within the system.
        /// </summary>
        public virtual string Flag { get; set; }

        /// <summary>
        /// Holds a textual description for the flag, optional.
        /// </summary>
        public virtual string Description { get; set; }
    }
}
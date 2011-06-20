/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 20011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;

namespace SpikeLite.Domain.Model.People
{
    /// <summary>
    /// Holds a single factoid about a given person. 
    /// </summary>
    public class PersonFactoid
    {
        /// <summary>
        /// Gets or sets the pkey, which is an incrementing id.
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Holds the person this factoid is associated with.
        /// </summary>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Holds the type of this factoid. This is merely a tag allowing categorizations of factoids.
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Holds the description of the factoid. An example of which would be information on why a user would be banned, or warned.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Holds when the factoid was created. Can be handy for ordering, or filtering.
        /// </summary>
        public virtual DateTime CreationDate { get; set; }
    }
}

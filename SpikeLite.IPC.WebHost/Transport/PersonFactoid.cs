/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2012 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System;

namespace SpikeLite.IPC.WebHost.Transport
{
    /// <summary>
    /// Provides a transport model representation of the domain model version of a factoid related to a person. In this case we don't have any cycles in our 
    /// object graph.
    /// </summary>
    public class PersonFactoid
    {
        /// <summary>
        /// Gets or sets the pkey, which is an incrementing id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Holds the type of this factoid. This is merely a tag allowing categorizations of factoids.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Holds the description of the factoid. An example of which would be information on why a user would be banned, or warned.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Holds when the factoid was created. Can be handy for ordering, or filtering.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Holds the nick of the user that created the factoid.
        /// </summary>
        public string CreatedBy { get; set; }
    }
}

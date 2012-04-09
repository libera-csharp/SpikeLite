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
    /// Provides a transport model representation of the domain model version of a person. In this case we don't have any cycles in our object graph,
    /// and some of the collections are arrays (Mono has some troubles serializing non-array type collections).
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Gets or sets the ID of the person.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Holds the name of the person. This can be any literal string, but any two factoids for the same literal string will be grouped.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Holds the factoids that are associated with this person.
        /// </summary>
        public PersonFactoid[] Factoids { get; set; }
    }
}

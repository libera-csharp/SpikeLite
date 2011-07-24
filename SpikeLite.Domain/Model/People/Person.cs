/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;

namespace SpikeLite.Domain.Model.People
{
    /// <summary>
    /// Represents a person, and a set of facts about them.
    /// </summary>
    /// 
    /// <remarks>
    /// The usage of the word person here is not literal: since it can be hard to determine identity absolutely on IRC, a person is
    /// in this case just a literal string of text (case-sensitive). You can give a hostmask, a nickname or anything else you'd like.
    /// </remarks>
    public class Person
    {
        /// <summary>
        /// Holds a backing list of factoids. This is not an automatic property because the collection needs to be initialized.
        /// </summary>
        private ICollection<PersonFactoid> _factoidBacking = new List<PersonFactoid>();

        /// <summary>
        /// Gets or sets the pkey, which is an incrementing id.
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// Holds the name of the person. This can be any literal string, but any two factoids for the same literal string will be grouped.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the associated factoids. May be empty, but never null.
        /// </summary>
        public virtual ICollection<PersonFactoid> Factoids
        {
            get { return _factoidBacking; }
            set { _factoidBacking = value; }
        }
    }
}

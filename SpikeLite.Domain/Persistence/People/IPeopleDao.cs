/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using SpikeLite.Domain.Model.People;

namespace SpikeLite.Domain.Persistence.People
{
    /// <summary>
    /// Provides a contract for a DAO that interacts with <see cref="Person"/> entities.
    /// </summary>
    public interface IPeopleDao
    {
        /// <summary>
        /// Finds all the people known to the system.
        /// </summary>
        /// 
        /// <returns>A collection of all people known to the system.</returns>
        IList<Person> FindAllPeople();

        /// <summary>
        /// Does much what it says on the tin: either creates (and persists) a new <see cref="Person"/> or pulls back a currently existing person by said name.
        /// </summary>
        /// 
        /// <param name="name">The name of the person to look for.</param>
        /// 
        /// <returns>Either a freshly persisted <see cref="Person"/> with an empty collection of factoids, or a pre-existing one with a complete object graph of factoids.</returns>
        Person CreateOrFindPerson(string name);

        /// <summary>
        /// Appends a factoid to the given name.
        /// </summary>
        /// 
        /// <param name="factoid">A fully-formed <see cref="PersonFactoid"/> to persist.</param>
        /// 
        /// <remarks>
        /// Because we may not have the entire <see cref="Person"/> with all the associated factoids faulted into memory, we'll just try
        /// and save the factoid by itself.
        /// </remarks>
        void SaveFactoid(PersonFactoid factoid);
    }
}

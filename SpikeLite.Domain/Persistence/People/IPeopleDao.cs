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
        /// Saves the factoids for a given user.
        /// </summary>
        /// 
        /// <param name="factoid">A fully-formed <see cref="Person"/> to persist.</param>
        void SaveFactoids(Person factoid);

        /// <summary>
        /// Deletes a factoid, by ID.
        /// </summary>
        /// 
        /// <param name="factoidId">The ID of the factoid to delete.</param>
        void DeleteFactoidById(int factoidId);
    }
}

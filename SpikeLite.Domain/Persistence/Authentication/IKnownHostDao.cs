/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Domain.Persistence.Authentication
{
    /// <summary>
    /// Provides a contract for what our known host DAO can do.
    /// </summary>
    public interface IKnownHostDao
    {
        /// <summary>
        /// Attempts to find all known hosts and provide an enumeration. This comes in handy for caching
        /// the set of known users, like we do on startup.
        /// </summary>
        /// 
        /// <returns>An enumeration of known hosts. May be empty, but will not be null.</returns>
        IList<KnownHost> FindAll();

        /// <summary>
        /// Attempts to stock a base set of ACLs.
        /// </summary>
        void SeedAcLs();

        /// <summary>
        /// Attempts to persist a given <see cref="KnownHost"/>.
        /// </summary>
        /// 
        /// <param name="host">An implementation of <see cref="KnownHost"/> to persist.</param>
        void Save(KnownHost host);

        /// <summary>
        /// Attempts to delete a given <see cref="KnownHost"/>.
        /// </summary>
        /// 
        /// <param name="host">An implementation of <see cref="KnownHost"/> to delete.</param>
        void Delete(KnownHost host);
    }
}
/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using SpikeLite.Domain.Model.Authentication;

namespace SpikeLite.Domain.Persistence.Authentication
{
    /// <summary>
    /// Provides a contract for creating and manipulating access flags within the system.
    /// </summary>
    public interface IAccessFlagDao
    {
        /// <summary>
        /// Finds all access flags within the system.
        /// </summary>
        /// 
        /// <returns>The set of access flags within the system.</returns>
        IList<AccessFlag> FindAll();

        /// <summary>
        /// Creates a new access flag.
        /// </summary>
        /// 
        /// <param name="flagIdentifier">An identifier for the flag: namely the identifier that you'd use later on for saying "this resource requires X permission".</param>
        /// <param name="flagDescription">An optional description for the flag.</param>
        AccessFlag CreateFlag(string flagIdentifier, string flagDescription);

        /// <summary>
        /// Attempts to either save the flag, or update it.
        /// </summary>
        /// 
        /// <param name="accessFlag">A valid <see cref="AccessFlag"/>. Must have a unique identifier.</param>
        /// 
        /// <remarks>If the flag identifier for the flag is already in use within the system, this will thorw an exception.</remarks>
        void SaveOrUpdate(AccessFlag accessFlag);
    }
}

/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2009 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
 
namespace SpikeLite.Persistence.Karma
{
    /// <summary>
    /// This interface provides a contract for what our karma dao does.
    /// </summary>
    public interface IKarmaDao
    {
        /// <summary>
        /// Attempts to find a Karma item for a given username. 
        /// </summary>
        /// 
        /// <param name="userName">The username to attempt to find a karma entry for.</param>
        /// 
        /// <returns>A karma item if the user is known, else null.</returns>
        KarmaItem FindKarma(string userName);

        // TODO: Kog 12/25/2008 - We should get this for free from the generics-based HibernateDaoSupport. I think.

        /// <summary>
        /// Attempts to persist our Karma item.
        /// </summary>
        /// 
        /// <param name="karma">The karma item to persist.</param>
        void SaveKarma(KarmaItem karma);
    }
}

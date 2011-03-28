/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using SpikeLite.Domain.Model.Karma;
using Spring.Data.NHibernate.Generic.Support;
using System.Linq;

namespace SpikeLite.Domain.Persistence.Karma
{
    /// <summary>
    /// A very light, cheap wrapper around KarmaItems
    /// </summary>
    public class KarmaDao : HibernateDaoSupport, IKarmaDao
    {
        /// <summary>
        /// Attempt to find karma for a given user
        /// </summary>
        /// 
        /// <param name="userName">Username to look for</param>
        /// 
        /// <returns>A <see cref="KarmaItem"/> for your username, or null if no curent entries</returns>
        public virtual KarmaItem FindKarma(string userName)
        {
            return HibernateTemplate.ExecuteFind(x => x.CreateQuery("from KarmaItem k where k.UserName = ?")
                                                                    .SetParameter(0, userName)
                                                                    .List<KarmaItem>()).FirstOrDefault();
        }

        /// <summary>
        /// Persist a <see cref="KarmaItem"/>
        /// </summary>
        /// 
        /// <param name="karma">Karma item to save</param>
        /// 
        /// <remarks>
        /// This will flush the session after saving.
        /// </remarks>
        public virtual void SaveKarma(KarmaItem karma)
        {
            HibernateTemplate.SaveOrUpdate(karma);
        }
    }
}
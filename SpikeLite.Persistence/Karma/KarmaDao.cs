/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Spring.Data.NHibernate.Support;

namespace SpikeLite.Persistence.Karma
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
            ISession sess = DoGetSession(false);

            ICriteria query = sess.CreateCriteria(typeof(KarmaItem))
                                .Add(Restrictions.Eq("UserName", userName));

            IList<KarmaItem> karmaItems = query.List<KarmaItem>();

            return (karmaItems.Count > 0) ? karmaItems[0] : null;
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
            DoGetSession(false).SaveOrUpdateCopy(karma);
        }
    }
}

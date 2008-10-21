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

namespace SpikeLite.Persistence.Karma
{
    // TODO: Kog - 07/25/2008 - check the need for flushes, see if we can use an interceptor
    // TODO: Kog - 07/25/2008 - inject our factory, or session into the DAO

    /// <summary>
    /// A very light, cheap wrapper around KarmaItems
    /// </summary>
    public class KarmaDao
    {
        public PersistenceLayer Persistence { get; set; }

        public KarmaDao(PersistenceLayer persistenceLayer)
        {
            Persistence = persistenceLayer;
        }

        /// <summary>
        /// Attempt to find karma for a given user
        /// </summary>
        /// 
        /// <param name="userName">Username to look for</param>
        /// 
        /// <returns>A <see cref="KarmaItem"/> for your username, or null if no curent entries</returns>
        public KarmaItem findKarma(string userName)
        {
            ISession sess = Persistence.Session;

            ICriteria query = sess.CreateCriteria(typeof(KarmaItem)).Add(Restrictions.Eq("UserName", userName));
            sess.Flush();

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
        public void saveKarma(KarmaItem karma)
        {
            Persistence.Session.Save(karma);
            Persistence.Session.Flush();
        }
    }
}

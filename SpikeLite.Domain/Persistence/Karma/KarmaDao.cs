/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Linq;
using SpikeLite.Domain.Model.Karma;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.Karma
{
    /// <summary>
    /// A very light, cheap wrapper around KarmaItems
    /// </summary>
    public class KarmaDao : HibernateDaoSupport, IKarmaDao
    {
        //private IPersistanceSession _persistanceSession = new RavenDBSession();
        private IPersistanceSession _persistanceSession;

        /// <summary>
        /// Attempt to find karma for a given user
        /// </summary>
        /// 
        /// <param name="userName">Username to look for</param>
        /// 
        /// <returns>A <see cref="KarmaItem"/> for your username, or null if no curent entries</returns>
        public virtual KarmaItem FindKarma(string userName)
        {
            _persistanceSession = new NHibernateSession(this.Session);

            return _persistanceSession.Query<KarmaItem>().SingleOrDefault(k => k.UserName == userName);
        }

        /// <summary>
        /// Persist a <see cref="KarmaItem"/>
        /// </summary>
        /// 
        /// <param name="entity">Karma item to save</param>
        /// 
        /// <remarks>
        /// This will flush the session after saving.
        /// </remarks>
        public virtual void SaveKarma(KarmaItem entity)
        {
            _persistanceSession = new NHibernateSession(this.Session);

            _persistanceSession.Update(entity);
            _persistanceSession.SaveChanges();
        }
    }
}
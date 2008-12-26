/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;
using NHibernate;
using Spring.Data.NHibernate.Support;

namespace SpikeLite.Persistence.Authentication
{
    /// <summary>
    /// This class represents a cheap DAO to do DAO-like things upon known hosts (previously 'cloaks').
    /// We're not very exciting at present, but then our ORM handles all the CRUD ops that we used to
    /// hand-write.
    /// </summary>
    public class KnownHostDao : HibernateDaoSupport, IKnownHostDao
    {
        /// <summary>
        /// Attempt to find all known hosts in our ACL database. 
        /// </summary>
        /// 
        /// <returns>0 or more known hosts.</returns>
        /// 
        /// <remarks>
        /// We do not currently use any caching mechanism, and instead make use of an in-memory copy within
        /// our ACL manager. There doesn't seem to be much value in any other strategy.
        /// </remarks>
        public virtual IEnumerable<KnownHost> FindAll()
        {
            ISession sess = DoGetSession(false);

            ICriteria query = sess.CreateCriteria(typeof(KnownHost));
            IList<KnownHost> hostList = query.List<KnownHost>();

            // Yeah, this sucks, we need to get rid of it ASAP.
            if (hostList.Count < 1)
            {
                SeedACLs();
                hostList = query.List<KnownHost>();
            }

            return hostList;
        }

        /// <summary>
        /// When we do not have a database on hand we call this method to seed some accounts.
        /// </summary>
        private void SeedACLs()
        {
            ISession sess = DoGetSession(false);

            // Get most of the regulars
            sess.Save(new KnownHost
            {
                 AccessLevel = AccessLevel.Public,
                 HostMatchType = HostMatchType.Start,
                 HostMask = "about/csharp/"
            });

            // Take care of smippy
            sess.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "about/csharp/regular/smellyhippy"
            });

            // Take care of KoTS
            sess.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "about/csharp/regular/KeeperOfTheSoul"
            });

            // Take care of Kog
            sess.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "about/csharp/regular/Kog"
            });

            // Take care of a test user
            sess.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "doot.doot"
            });
        }
    }
}

/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.Collections.Generic;
using NHibernate;

namespace SpikeLite.Persistence.Authentication
{
    /// <summary>
    /// This class represents a cheap DAO to do DAO-like things upon known hosts (previously 'cloaks').
    /// We're not very exciting at present, but then our ORM handles all the CRUD ops that we used to
    /// hand-write.
    /// </summary>
    public class KnownHostDao
    {
        public PersistenceLayer Persistence { get; set; }
        
        /// <summary>
        /// Creates an instance of our DAO using constructor injection on our persistence layer.
        /// </summary>
        /// 
        /// <param name="persistenceLayer">A persistence layer.</param>
        public KnownHostDao(PersistenceLayer persistenceLayer)
        {
            Persistence = persistenceLayer;
        }

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
        public IEnumerable<KnownHost> FindAll()
        {
            ISession sess = Persistence.Session;

            ICriteria query = sess.CreateCriteria(typeof(KnownHost));
            sess.Flush();

            return query.List<KnownHost>();
        }

        /// <summary>
        /// When we do not have a database on hand we call this method to seed some accounts.
        /// </summary>
        public static void SeedACLs(ISession session)
        {
            // Get most of the regulars
            session.Save(new KnownHost
            {
                 AccessLevel = AccessLevel.Public,
                 HostMatchType = HostMatchType.Start,
                 HostMask = "about/csharp/"
            });

            // Take care of smippy
            session.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "about/csharp/regular/smellyhippy"
            });

            // Take care of KoTS
            session.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "about/csharp/regular/KeeperOfTheSoul"
            });

            // Take care of Kog
            session.Save(new KnownHost
            {
                AccessLevel = AccessLevel.Root,
                HostMatchType = HostMatchType.Start,
                HostMask = "about/csharp/regular/Kog"
            });

            session.Flush();
        }
    }
}

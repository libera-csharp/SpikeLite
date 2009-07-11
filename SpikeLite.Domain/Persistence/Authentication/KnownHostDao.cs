/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using SpikeLite.Domain.Model.Authentication;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.Authentication
{
    /// <summary>
    /// This class represents a cheap DAO to do DAO-like things upon known hosts (previously 'cloaks').
    /// We're not very exciting at present, but then our ORM handles all the CRUD ops that we used to
    /// hand-write.
    /// </summary>
    public class KnownHostDao : HibernateDaoSupport, IKnownHostDao
    {
        // TODO: Kog 07/10/2009 - We should be able to do stuff like generic parameterless CRUD from either hibernate template or we need to
        // TODO:                  to implement an abstract DAO that we can wrap it with.

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
        public virtual IList<KnownHost> FindAll()
        {
            return HibernateTemplate.ExecuteFind(x => x.CreateCriteria(typeof(KnownHost)).List<KnownHost>());
        }

        /// <summary>
        /// When we do not have a database on hand we call this method to seed some accounts.
        /// </summary>
        public virtual void SeedAcLs()
        {
            // Get most of the regulars
            HibernateTemplate.Save(new KnownHost
                                   {
                                       AccessLevel = AccessLevel.Public,
                                       HostMatchType = HostMatchType.Start,
                                       HostMask = "about/csharp/"
                                   });

            // Take care of smippy
            HibernateTemplate.Save(new KnownHost
                                   {
                                       AccessLevel = AccessLevel.Root,
                                       HostMatchType = HostMatchType.Start,
                                       HostMask = "about/csharp/regular/smellyhippy"
                                   });

            // Take care of KoTS
            HibernateTemplate.Save(new KnownHost
                                   {
                                       AccessLevel = AccessLevel.Root,
                                       HostMatchType = HostMatchType.Start,
                                       HostMask = "about/csharp/regular/KeeperOfTheSoul"
                                   });

            // Take care of Kog
            HibernateTemplate.Save(new KnownHost
                                   {
                                       AccessLevel = AccessLevel.Root,
                                       HostMatchType = HostMatchType.Start,
                                       HostMask = "about/csharp/regular/Kog"
                                   });

            // Take care of a test user
            HibernateTemplate.Save(new KnownHost
                                   {
                                       AccessLevel = AccessLevel.Root,
                                       HostMatchType = HostMatchType.Start,
                                       HostMask = "doot.doot"
                                   });
        }
    }
}
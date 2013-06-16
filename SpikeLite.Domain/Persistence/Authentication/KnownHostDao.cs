/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008-2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using System.Linq;
using SpikeLite.Domain.Model.Authentication;
using Spring.Data.NHibernate.Generic.Support;

// TODO: Kog 07/10/2009 - We should be able to do stuff like generic parameterless CRUD from either hibernate template or we need to
// TODO:                  to implement an abstract DAO that we can wrap it with.

namespace SpikeLite.Domain.Persistence.Authentication
{
    /// <summary>
    /// This class represents a cheap DAO to do DAO-like things upon known hosts (previously 'cloaks').
    /// We're not very exciting at present, but then our ORM handles all the CRUD ops that we used to
    /// hand-write.
    /// </summary>
    public class KnownHostDao : HibernateDaoSupport, IKnownHostDao
    {
        //private IPersistanceSession _persistanceSession = new RavenDBSession();
        private IPersistanceSession _persistanceSession = null;

        public virtual IList<KnownHost> FindAll()
        {
            _persistanceSession = new NHibernateSession(this.Session);

            return _persistanceSession.Query<KnownHost>().ToList();
        }

        public void SaveOrUpdate(KnownHost entity)
        {
            _persistanceSession = new NHibernateSession(this.Session);

            _persistanceSession.Update(entity);
            _persistanceSession.SaveChanges();
        }

        public void Delete(KnownHost host) { }
    }
}
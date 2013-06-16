/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using System.Linq;
using log4net.Ext.Trace;
using SpikeLite.Domain.Model.Authentication;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.Authentication
{
    public class AccessFlagDao : HibernateDaoSupport, IAccessFlagDao
    {
        //private IPersistanceSession _persistanceSession = new RavenDBSession();
        private IPersistanceSession _persistanceSession;

        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private static readonly TraceLogImpl Logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(AccessFlagDao));

        public IList<AccessFlag> FindAll()
        {
            _persistanceSession = new NHibernateSession(this.Session);

            return _persistanceSession.Query<AccessFlag>().ToList();
        }

        public AccessFlag CreateFlag(string flagIdentifier, string flagDescription)
        {
            return new AccessFlag {Flag = flagIdentifier, Description = flagDescription};
        }

        public void SaveOrUpdate(AccessFlag accessFlag)
        {
            _persistanceSession = new NHibernateSession(this.Session);

            _persistanceSession.Add(accessFlag);
            _persistanceSession.SaveChanges();
        }
    }
}
/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2011 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */

using System.Collections.Generic;
using log4net.Ext.Trace;
using SpikeLite.Domain.Model.Authentication;
using Spring.Data.NHibernate.Generic.Support;

namespace SpikeLite.Domain.Persistence.Authentication
{
    public class AccessFlagDao : HibernateDaoSupport, IAccessFlagDao
    {
        /// <summary>
        /// Stores our log4net logger.
        /// </summary>
        private static readonly TraceLogImpl Logger = (TraceLogImpl)TraceLogManager.GetLogger(typeof(AccessFlagDao));

        public IList<AccessFlag> FindAll()
        {
            return HibernateTemplate.ExecuteFind(x => x.CreateCriteria(typeof(AccessFlag)).List<AccessFlag>());
        }

        public AccessFlag CreateFlag(string flagIdentifier, string flagDescription)
        {
            return new AccessFlag {Flag = flagIdentifier, Description = flagDescription};
        }

        public void SaveOrUpdate(AccessFlag accessFlag)
        {
            HibernateTemplate.SaveOrUpdate(accessFlag);

            // Adding new access flags is indeed of significance. We should log this.
            Logger.InfoFormat("Created new access flag {0} ({1}), with description {2}.", accessFlag.Flag, accessFlag.Id, accessFlag.Description);
        }
    }
}

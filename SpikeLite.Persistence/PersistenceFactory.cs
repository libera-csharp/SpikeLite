/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System.IO;
using System.Reflection;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.Data.SQLite;

namespace SpikeLite.Persistence
{
    // TODO: Kog - 7/25/2008 - inject the name of our config file

    /// <summary>
    /// A factory to get us our persistence information
    /// </summary>
    public class PersistenceFactory
    {
        /// <summary>
        /// Attempts to set up our persistence. This will create new tables if the sdb3 file is missing,
        /// else use what's already there.
        /// </summary>
        /// 
        /// <returns>A <see cref="PersistenceLayer"/> object we can query for sessions</returns>
        public static PersistenceLayer getPersistence()
        {
            // Config it.
            Configuration cfg = new Configuration();
            cfg.Configure(Assembly.GetExecutingAssembly(), "SpikeLite.Persistence.NHibernate.NHibernateProvider.config");
            cfg.AddAssembly("SpikeLite.Persistence");

            // Create our schema from our mappings, if necessary.
            if (!File.Exists("persistence.s3db"))
            {
                SQLiteConnection.CreateFile("persistence.s3db");
                new SchemaExport(cfg).Execute(true, true, false, false);
            }

            // Create connection.
            return new PersistenceLayer(cfg.BuildSessionFactory().OpenSession());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Embedded;

namespace SpikeLite.Domain.Persistence
{
    internal static class RavenDatabase
    {
        private static readonly IDocumentStore documentStore;
        static RavenDatabase()
        {
            documentStore = new EmbeddableDocumentStore
            {
                DataDirectory = "Data",
                UseEmbeddedHttpServer = true
            }.Initialize();
        }

        internal static IList<T> GetAll<T>()
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                List<T> allEntities = new List<T>();
                int start = 0;

                while (true)
                {
                    var someEntities = session
                        .Query<T>()
                        .Take(1024)
                        .Skip(start)
                        .ToList();

                    if (someEntities.Count == 0)
                        break;

                    start += someEntities.Count;
                    allEntities.AddRange(someEntities);
                }

                session.SaveChanges();

                return allEntities;
            }
        }

        internal static T GetById<T>(string id)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Load<T>(id);
            }
        }

        internal static T GetOrCreate<T>(string id, Func<T> createNew)
            where T : class
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                return session.Load<T>(id) ?? createNew();
            }
        }

        internal static void Save<T>(T entity)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                session.Store(entity);
                session.SaveChanges();
            }
        }

        internal static void Save<T>(T entity, Func<T, string> getId)
        {
            using (IDocumentSession session = documentStore.OpenSession())
            {
                session.Store(entity, getId(entity));
                session.SaveChanges();
            }
        }
    }
}
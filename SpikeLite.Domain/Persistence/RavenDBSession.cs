using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using Raven.Client.Embedded;

namespace SpikeLite.Domain.Persistence
{
    public class RavenDBSession : IPersistanceSession
    {
        private static readonly IDocumentStore documentStore;
        static RavenDBSession()
        {
            //TODO: AJ: Make this configurable
            documentStore = new EmbeddableDocumentStore
            {
                DataDirectory = "Data",
                UseEmbeddedHttpServer = true
            }.Initialize();
        }

        private IDocumentSession _documentSession = documentStore.OpenSession();

        public IQueryable<TEntity> Query<TEntity>()
        {
            return _documentSession.Query<TEntity>();
        }

        public void Add<TEntity>(TEntity entity)
        {
            _documentSession.Store(entity);
        }

        public void Update<TEntity>(TEntity entity)
        {
            _documentSession.Store(entity);
        }

        public void Delete<TEntity>(TEntity entity)
        {
            _documentSession.Delete(entity);
        }

        public void SaveChanges()
        {
            _documentSession.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _documentSession.Dispose();
                _documentSession = null;
            }
        }
    }
}
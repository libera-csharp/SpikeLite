using System;
using System.Linq;
using Spring.Data.NHibernate.Generic.Support;
using NHibernate.Linq;
using NHibernate;

namespace SpikeLite.Domain.Persistence
{
    public class NHibernateSession : IPersistanceSession
    {
        private ISession Session;

        public NHibernateSession(ISession session)
        {
            this.Session = session;
        }

        public IQueryable<TEntity> Query<TEntity>()
        {
            return Session.Query<TEntity>();
        }

        public void Add<TEntity>(TEntity entity)
        {
            Session.Save(entity);
        }

        public void Update<TEntity>(TEntity entity)
        {
            Session.Update(entity);
        }

        public void Delete<TEntity>(TEntity entity)
        {
            Session.Delete(entity);
        }

        public void SaveChanges()
        {
            var transaction = Session.Transaction;

            if (transaction != null && transaction.IsActive)
                Session.Transaction.Commit();
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
            }
        }
    }
}
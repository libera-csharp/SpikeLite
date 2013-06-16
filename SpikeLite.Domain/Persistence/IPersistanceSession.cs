using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpikeLite.Domain.Persistence
{
    public interface IPersistanceSession : IDisposable
    {
        IQueryable<TEntity> Query<TEntity>();
        void Add<TEntity>(TEntity entity);
        void Update<TEntity>(TEntity entity);
        void Delete<TEntity>(TEntity entity);
        void SaveChanges();
    }
}
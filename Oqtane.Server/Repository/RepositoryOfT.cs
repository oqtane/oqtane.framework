using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Repository
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity: class, new()
    {
        private readonly DbContext _context;

        public Repository(DbContext context)
        {
            _context = context;
        }

        public virtual IEnumerable<TEntity> GetAll() => _context.Set<TEntity>().ToList();

        public virtual TEntity Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
            _context.SaveChanges();

            return entity;
        }

        public virtual TEntity Update(TEntity entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();

            return entity;
        }

        public virtual TEntity Get(int id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public virtual void Delete(int id)
        {
            var tenant = _context.Set<TEntity>().Find(id);
            _context.Set<TEntity>().Remove(tenant);
            _context.SaveChanges();
        }
    }
}

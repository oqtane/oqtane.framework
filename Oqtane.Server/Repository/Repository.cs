using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Repository
{
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity: class, new()
    {
        private readonly DbContext _context;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = _context.Set<TEntity>();
        }

        protected DbSet<TEntity> DbSet { get; }

        public virtual IQueryable<TEntity> GetAll() => DbSet;

        public virtual TEntity Add(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            DbSet.Add(entity);
            _context.SaveChanges();

            return entity;
        }

        public virtual TEntity Update(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();

            return entity;
        }

        public virtual TEntity Get(int id)
        {
            return DbSet.Find(id);
        }

        public virtual void Delete(int id)
        {
            var entity = DbSet.Find(id);
            DbSet.Remove(entity);
            _context.SaveChanges();
        }
    }
}

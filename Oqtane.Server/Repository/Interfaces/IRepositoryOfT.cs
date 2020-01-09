using System.Collections.Generic;

namespace Oqtane.Repository
{
    public interface IRepository<TEntity> : IRepository where TEntity: class, new()
    {
        TEntity Add(TEntity entity);

        void Delete(int id);

        TEntity Update(TEntity entity);

        TEntity Get(int id);

        IEnumerable<TEntity> GetAll();
    }
}

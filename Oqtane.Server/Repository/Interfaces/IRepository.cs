using System.Linq;

namespace Oqtane.Repository
{
    public interface IRepository<TEntity> where TEntity: class, new()
    {
        TEntity Add(TEntity entity);

        void Delete(int id);

        TEntity Update(TEntity entity);

        TEntity Get(int id);

        IQueryable<TEntity> GetAll();
    }
}

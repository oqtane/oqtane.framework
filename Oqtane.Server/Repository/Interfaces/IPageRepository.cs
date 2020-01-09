using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageRepository : IRepository<Page>
    {
        Page Get(int id, int userId);

        IEnumerable<Page> GetAll(int siteId);
    }
}

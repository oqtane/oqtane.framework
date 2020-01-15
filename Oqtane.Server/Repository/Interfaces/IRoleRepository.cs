using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IRoleRepository : IRepository<Role>
    {
        IEnumerable<Role> GetAll(int siteId);

        IEnumerable<Role> GetAll(int siteId, bool includeGlobalRoles);
    }
}

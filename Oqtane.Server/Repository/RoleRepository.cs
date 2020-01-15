using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class RoleRepository : Repository<Role>, IRepository<Role>
    {
        public RoleRepository(TenantDBContext context)
            : base(context)
        {

        }

        public IEnumerable<Role> GetRoles(int siteId)
            => DbSet.Where(item => item.SiteId == siteId);

        public IEnumerable<Role> GetRoles(int siteId, bool includeGlobalRoles)
            => DbSet.Where(item => item.SiteId == siteId || item.SiteId == null);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using [Owner].[Module]s.Models;

namespace [Owner].[Module]s.Repository
{
    public class [Module]Context : DBContextBase, IService
    {
        public virtual DbSet<[Module]> [Module] { get; set; }

        public [Module]Context(ITenantResolver tenantResolver, IHttpContextAccessor accessor) : base(tenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

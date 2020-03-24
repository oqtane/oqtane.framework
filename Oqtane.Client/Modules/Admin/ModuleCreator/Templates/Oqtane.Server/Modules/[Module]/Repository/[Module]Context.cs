using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Models.[Module]s;
using Oqtane.Modules;

namespace Oqtane.Repository.[Module]s
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

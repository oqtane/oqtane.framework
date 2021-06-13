using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Oqtane.Modules;
using Oqtane.Repository;
using Oqtane.Infrastructure;
using Oqtane.Repository.Databases.Interfaces;

namespace [Owner].[Module].Repository
{
    public class [Module]Context : DBContextBase, IService, IMultiDatabase
    {
        public virtual DbSet<Models.[Module]> [Module] { get; set; }

        public [Module]Context(ITenantManager tenantManager, IHttpContextAccessor accessor) : base(tenantManager, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

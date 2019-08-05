using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class TenantDBContext : DBContextBase
    {
        public virtual DbSet<Site> Site { get; set; }
        public virtual DbSet<Page> Page { get; set; }
        public virtual DbSet<PageModule> PageModule { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<SiteUser> SiteUser { get; set; }

        public TenantDBContext(ITenantResolver TenantResolver, IHttpContextAccessor accessor) : base(TenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }

    }
}

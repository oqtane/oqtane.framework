using Microsoft.EntityFrameworkCore;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextContext : DBContextBase, IService
    {
        public virtual DbSet<HtmlTextInfo> HtmlText { get; set; }

        public HtmlTextContext(ITenantResolver tenantResolver, IHttpContextAccessor accessor, IConfiguration configuration) : base(tenantResolver, accessor, configuration)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

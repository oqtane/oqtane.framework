using Microsoft.EntityFrameworkCore;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Repository;
using Microsoft.AspNetCore.Http;

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextContext : DBContextBase, IService
    {
        public virtual DbSet<HtmlTextInfo> HtmlText { get; set; }

        public HtmlTextContext(ITenantResolver TenantResolver, IHttpContextAccessor accessor) : base(TenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

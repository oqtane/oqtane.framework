using Microsoft.EntityFrameworkCore;
using Oqtane.Shared.Modules.HtmlText.Models;
using Oqtane.Repository;
using Oqtane.Modules;

namespace Oqtane.Server.Modules.HtmlText.Repository
{
    public class HtmlTextContext : ContextBase, IService
    {
        public virtual DbSet<HtmlTextInfo> HtmlText { get; set; }

        public HtmlTextContext(ITenantResolver TenantResolver):base(TenantResolver)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

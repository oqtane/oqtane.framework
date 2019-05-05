using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared.Modules.HtmlText.Models;
using Oqtane.Repository;
using Oqtane.Modules;

namespace Oqtane.Server.Modules.HtmlText.Repository
{
    public class HtmlTextContext : ContextBase, IService
    {
        public virtual DbSet<HtmlTextInfo> HtmlText { get; set; }

        public HtmlTextContext(ITenantRepository TenantRepository):base(TenantRepository)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

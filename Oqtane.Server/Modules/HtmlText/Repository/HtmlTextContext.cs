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

        public HtmlTextContext(IDbConfig dbConfig, ITenantResolver tenantResolver) : base(dbConfig, tenantResolver)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

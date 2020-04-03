using Microsoft.EntityFrameworkCore;
using Oqtane.Repository;
using Microsoft.AspNetCore.Http;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Modules.Models.HtmlText;
using Oqtane.Repository.Context;
using Oqtane.Repository.Interfaces;

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextContext : DBContextBase, IService
    {
        public virtual DbSet<HtmlTextInfo> HtmlText { get; set; }

        public HtmlTextContext(ITenantResolver tenantResolver, IHttpContextAccessor accessor) : base(tenantResolver, accessor)
        {
            // ContextBase handles multi-tenant database connections
        }
    }
}

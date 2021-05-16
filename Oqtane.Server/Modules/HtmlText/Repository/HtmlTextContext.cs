using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Oqtane.Database;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextContext : DBContextBase, IService, IMultiDatabase
    {
        public HtmlTextContext(ITenantManager tenantManager, IHttpContextAccessor httpContextAccessor) : base(tenantManager, httpContextAccessor) { }

        public virtual DbSet<Models.HtmlText> HtmlText { get; set; }
    }
}

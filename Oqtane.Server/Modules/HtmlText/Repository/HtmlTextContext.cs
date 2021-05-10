using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Repository.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextContext : DBContextBase, IService, IMultiDatabase
    {
        public HtmlTextContext(IDbConfig dbConfig, ITenantManager tenantManager) : base(dbConfig, tenantManager)
        {
        }

        public virtual DbSet<Models.HtmlText> HtmlText { get; set; }
    }
}

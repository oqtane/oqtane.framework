using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Oqtane.Modules.HtmlText.Models;
using Oqtane.Repository;
using Oqtane.Interfaces;
using Oqtane.Repository.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Modules.HtmlText.Repository
{
    public class HtmlTextContext : DBContextBase, IService, IMultiDatabase
    {
        public HtmlTextContext(IDbConfig dbConfig, ITenantResolver tenantResolver) : base(dbConfig, tenantResolver)
        {
        }

        public virtual DbSet<HtmlTextInfo> HtmlText { get; set; }
    }
}

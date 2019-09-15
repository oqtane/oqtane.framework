using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace Oqtane.Repository
{
    public class TenantResolver : ITenantResolver
    {
        private MasterDBContext db;
        private readonly string aliasname;
        private readonly IAliasRepository _aliasrepository;
        private readonly ITenantRepository _tenantrepository;

        public TenantResolver(MasterDBContext context, IHttpContextAccessor accessor, IAliasRepository aliasrepository, ITenantRepository tenantrepository)
        {
            db = context;
            _aliasrepository = aliasrepository;
            _tenantrepository = tenantrepository;

            // get alias based on request context
            aliasname = accessor.HttpContext.Request.Host.Value;
            string path = accessor.HttpContext.Request.Path.Value;
            string[] segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && segments[0] == "api" && segments[1] != "~")
            {
                aliasname += "/" + segments[1];
            }
            if (aliasname.EndsWith("/"))
            {
                aliasname = aliasname.Substring(0, aliasname.Length - 1);
            }
        }

        public Alias GetAlias()
        {
            IEnumerable<Alias> aliases = _aliasrepository.GetAliases(); // cached
            return aliases.Where(item => item.Name == aliasname).FirstOrDefault();
        }

        public Tenant GetTenant()
        {
            IEnumerable<Tenant> tenants = _tenantrepository.GetTenants(); // cached
            return tenants.Where(item => item.TenantId == GetAlias().TenantId).FirstOrDefault();
        }
    }
}
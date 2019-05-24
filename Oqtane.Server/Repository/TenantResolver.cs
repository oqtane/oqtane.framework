using System;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Oqtane.Repository
{
    public class TenantResolver : ITenantResolver
    {
        private HostContext db;
        private readonly string aliasname;
        private readonly IAliasRepository _aliasrepository;
        private readonly ITenantRepository _tenantrepository;

        public TenantResolver(HostContext context, IHttpContextAccessor accessor, IAliasRepository aliasrepository, ITenantRepository tenantrepository)
        {
            db = context;
            _aliasrepository = aliasrepository;
            _tenantrepository = tenantrepository;

            // get alias based on request context
            aliasname = accessor.HttpContext.Request.Host.Value;
            string path = accessor.HttpContext.Request.Path.Value;
            string[] segments = path.Split('/');
            if (segments[1] != "~")
            {
                aliasname += "/" + segments[1];
            }
        }

        public Tenant GetTenant()
        {
            try
            {
                IEnumerable<Alias> aliases = _aliasrepository.GetAliases(); // cached
                Alias alias = aliases.Where(item => item.Name == aliasname).FirstOrDefault();
                IEnumerable<Tenant> tenants = _tenantrepository.GetTenants(); // cached
                return tenants.Where(item => item.TenantId == alias.TenantId).FirstOrDefault();
            }
            catch
            {
                throw;
            }
        }
    }
}
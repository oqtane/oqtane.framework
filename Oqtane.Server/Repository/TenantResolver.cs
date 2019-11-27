using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;
using Microsoft.AspNetCore.Http;
using System;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class TenantResolver : ITenantResolver
    {
        private MasterDBContext db;
        private readonly string aliasname;
        private readonly IAliasRepository Aliases;
        private readonly ITenantRepository Tenants;
        private readonly SiteState sitestate;

        public TenantResolver(MasterDBContext context, IHttpContextAccessor accessor, IAliasRepository Aliases, ITenantRepository Tenants, SiteState sitestate)
        {
            db = context;
            this.Aliases = Aliases;
            this.Tenants = Tenants;
            this.sitestate = sitestate;
            aliasname = "";

            // get alias based on request context
            if (accessor.HttpContext != null)
            {
                aliasname = accessor.HttpContext.Request.Host.Value;
                string path = accessor.HttpContext.Request.Path.Value;
                string[] segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 1 && segments[1] == "api" && segments[0] != "~")
                {
                    aliasname += "/" + segments[0];
                }
                if (aliasname.EndsWith("/"))
                {
                    aliasname = aliasname.Substring(0, aliasname.Length - 1);
                }
            }
            else  // background processes can pass in an alias using the SiteState service
            {
                if (sitestate != null)
                {
                    aliasname = sitestate.Alias.Name;
                }
            }
        }

        public Alias GetAlias()
        {
            IEnumerable<Alias> aliases = Aliases.GetAliases(); // cached
            return aliases.Where(item => item.Name == aliasname).FirstOrDefault();
        }

        public Tenant GetTenant()
        {
            Tenant tenant = null;
            Alias alias = GetAlias();
            if (alias != null)
            {
                IEnumerable<Tenant> tenants = Tenants.GetTenants(); // cached
                tenant = tenants.Where(item => item.TenantId == alias.TenantId).FirstOrDefault();
            }
            return tenant;
        }
    }
}
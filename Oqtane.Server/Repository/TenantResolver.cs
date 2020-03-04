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
        private readonly Alias alias = null;
        private readonly Tenant tenant = null;

        public TenantResolver(IHttpContextAccessor Accessor, IAliasRepository Aliases, ITenantRepository Tenants, SiteState SiteState)
        {
            int aliasid = -1;
            string aliasname = "";

            // get alias identifier based on request context
            if (Accessor.HttpContext != null)
            {
                // check if an alias is passed as a querystring parameter ( for cross tenant access )
                if (Accessor.HttpContext.Request.Query.ContainsKey("aliasid"))
                {
                    aliasid = int.Parse(Accessor.HttpContext.Request.Query["aliasid"]);
                }
                else // get the alias from the request url
                {
                    aliasname = Accessor.HttpContext.Request.Host.Value;
                    string path = Accessor.HttpContext.Request.Path.Value;
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
            }
            else  // background processes can pass in an alias using the SiteState service
            {
                if (SiteState != null)
                {
                    aliasid = SiteState.Alias.AliasId;
                }
            }

            // get the alias and tenant
            if (aliasid != -1 || aliasname != "")
            {
                IEnumerable<Alias> aliases = Aliases.GetAliases(); // cached
                IEnumerable<Tenant> tenants = Tenants.GetTenants(); // cached

                if (aliasid != -1)
                {
                    alias = aliases.Where(item => item.AliasId == aliasid).FirstOrDefault();
                }
                else
                {
                    alias = aliases.Where(item => item.Name == aliasname).FirstOrDefault();
                }
                if (alias != null)
                {
                    tenant = tenants.Where(item => item.TenantId == alias.TenantId).FirstOrDefault();
                }
            }
        }

        public Alias GetAlias()
        {
            return alias;
        }

        public Tenant GetTenant()
        {
            return tenant;
        }
    }
}
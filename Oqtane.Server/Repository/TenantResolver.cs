using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class TenantResolver : ITenantResolver
    {
        private readonly Alias _alias;
        private readonly Tenant _tenant;

        public TenantResolver(IHttpContextAccessor accessor, IAliasRepository aliasRepository, ITenantRepository tenantRepository, SiteState siteState)
        {
            int aliasId = -1;
            string aliasName = "";

            // get alias identifier based on request context
            if (accessor.HttpContext != null)
            {
                // check if an alias is passed as a querystring parameter ( for cross tenant access )
                if (accessor.HttpContext.Request.Query.ContainsKey("aliasid"))
                {
                    aliasId = int.Parse(accessor.HttpContext.Request.Query["aliasid"]);
                }
                else // get the alias from the request url
                {
                    aliasName = accessor.HttpContext.Request.Host.Value;
                    string path = accessor.HttpContext.Request.Path.Value;
                    string[] segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length > 1 && segments[1] == "api" && segments[0] != "~")
                    {
                        aliasName += "/" + segments[0];
                    }
                    if (aliasName.EndsWith("/"))
                    {
                        aliasName = aliasName.Substring(0, aliasName.Length - 1);
                    }
                }
            }
            else  // background processes can pass in an alias using the SiteState service
            {
                if (siteState != null)
                {
                    aliasId = siteState.Alias.AliasId;
                }
            }

            // get the alias and tenant
            if (aliasId != -1 || aliasName != "")
            {
                IEnumerable<Alias> aliases = aliasRepository.GetAliases(); // cached
                IEnumerable<Tenant> tenants = tenantRepository.GetTenants(); // cached

                if (aliasId != -1)
                {
                    _alias = aliases.FirstOrDefault(item => item.AliasId == aliasId);
                }
                else
                {
                    _alias = aliases.FirstOrDefault(item => item.Name == aliasName);
                }
                if (_alias != null)
                {
                    _tenant = tenants.FirstOrDefault(item => item.TenantId == _alias.TenantId);
                }
            }
        }

        public Alias GetAlias()
        {
            return _alias;
        }

        public Tenant GetTenant()
        {
            return _tenant;
        }
    }
}

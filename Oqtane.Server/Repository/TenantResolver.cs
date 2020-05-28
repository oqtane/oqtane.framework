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

            if (siteState != null && siteState.Alias != null)
            {
                // background processes can pass in an alias using the SiteState service
                _alias = siteState.Alias;
            }
            else
            {
                // get aliasid identifier based on request
                if (accessor.HttpContext != null)
                {
                    string[] segments = accessor.HttpContext.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length > 1 && (segments[1] == "api" || segments[1] == "pages") && segments[0] != "~")
                    {
                        aliasId = int.Parse(segments[0]);
                    }
                }

                // get the alias
                IEnumerable<Alias> aliases = aliasRepository.GetAliases().ToList(); // cached
                if (aliasId != -1)
                {
                    _alias = aliases.FirstOrDefault(item => item.AliasId == aliasId);
                }
            }

            if (_alias != null)
            {
                // get the tenant
                IEnumerable<Tenant> tenants = tenantRepository.GetTenants(); // cached
                _tenant = tenants.FirstOrDefault(item => item.TenantId == _alias.TenantId);
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

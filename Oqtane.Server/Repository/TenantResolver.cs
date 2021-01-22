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
        private readonly IHttpContextAccessor _accessor;
        private readonly IAliasRepository _aliasRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly SiteState _siteState;

        private Alias _alias;
        private Tenant _tenant;

        public TenantResolver(IHttpContextAccessor accessor, IAliasRepository aliasRepository, ITenantRepository tenantRepository, SiteState siteState)
        {
            _accessor = accessor;
            _aliasRepository = aliasRepository;
            _tenantRepository = tenantRepository;
            _siteState = siteState;
        }

        public Alias GetAlias()
        {
            if (_alias == null) ResolveTenant();
            return _alias;
        }

        public Tenant GetTenant()
        {
            if (_tenant == null) ResolveTenant();
            return _tenant;
        }

        private void ResolveTenant()
        {
            if (_siteState != null && _siteState.Alias != null)
            {
                // background processes can pass in an alias using the SiteState service
                _alias = _siteState.Alias;
            }
            else
            {
                int aliasId = -1;

                // get aliasid identifier based on request
                if (_accessor.HttpContext != null)
                {
                    string[] segments = _accessor.HttpContext.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length > 1 && (segments[1] == "api" || segments[1] == "pages") && segments[0] != "~")
                    {
                        aliasId = int.Parse(segments[0]);
                    }
                }

                // get the alias
                IEnumerable<Alias> aliases = _aliasRepository.GetAliases().ToList(); // cached
                if (aliasId != -1)
                {
                    _alias = aliases.FirstOrDefault(item => item.AliasId == aliasId);
                }
            }

            if (_alias != null)
            {
                // get the tenant
                IEnumerable<Tenant> tenants = _tenantRepository.GetTenants(); // cached
                _tenant = tenants.FirstOrDefault(item => item.TenantId == _alias.TenantId);
            }

        }
    }
}

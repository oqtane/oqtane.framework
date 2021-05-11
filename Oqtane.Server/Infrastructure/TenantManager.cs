using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class TenantManager : ITenantManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAliasRepository _aliasRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly SiteState _siteState;

        public TenantManager(IHttpContextAccessor httpContextAccessor, IAliasRepository aliasRepository, ITenantRepository tenantRepository, SiteState siteState)
        {
            _httpContextAccessor = httpContextAccessor;
            _aliasRepository = aliasRepository;
            _tenantRepository = tenantRepository;
            _siteState = siteState;
        }

        public Alias GetAlias()
        {
            Alias alias = null;

            if (_siteState != null && _siteState.Alias != null)
            {
                alias = _siteState.Alias;
            }
            else
            {
                // if there is http context
                if (_httpContextAccessor.HttpContext != null)
                {
                    // legacy support for client api requests which would include the alias as a path prefix ( ie. {alias}/api/[controller] )
                    int aliasId;
                    string[] segments = _httpContextAccessor.HttpContext.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length > 1 && (segments[1] == "api" || segments[1] == "pages") && int.TryParse(segments[0], out aliasId))
                    {
                        alias = _aliasRepository.GetAliases().ToList().FirstOrDefault(item => item.AliasId == aliasId);
                    }

                    // resolve alias based on host name and path
                    if (alias == null)
                    {
                        string name = _httpContextAccessor.HttpContext.Request.Host.Value + _httpContextAccessor.HttpContext.Request.Path;
                        alias = _aliasRepository.GetAlias(name);
                    }

                    // if there is a match save it
                    if (alias != null)
                    {
                        _siteState.Alias = alias;
                    }
                }
            }

            return alias;
        }

        public Tenant GetTenant()
        {
            var alias = GetAlias();
            if (alias != null)
            {
                // return tenant details
                return _tenantRepository.GetTenants().ToList().FirstOrDefault(item => item.TenantId == alias.TenantId);
            }
            return null;
        }

        public void SetAlias(Alias alias)
        {
            // background processes can set the alias using the SiteState service
            _siteState.Alias = alias;
        }

        public void SetTenant(int tenantId)
        {
            // background processes can set the alias using the SiteState service
            _siteState.Alias = new Alias { TenantId = tenantId };
        }
    }
}

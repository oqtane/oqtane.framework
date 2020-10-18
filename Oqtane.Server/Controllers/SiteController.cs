using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class SiteController : Controller
    {
        private readonly ISiteRepository _sites;
        private readonly ITenantResolver _tenants;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public SiteController(ISiteRepository sites, ITenantResolver tenants, ISyncManager syncManager, ILogManager logger)
        {
            _sites = sites;
            _tenants = tenants;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public IEnumerable<Site> Get()
        {
            return _sites.GetSites();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Site Get(int id)
        {
            return _sites.GetSite(id);
        }

        // POST api/<controller>
        [HttpPost]
        public Site Post([FromBody] Site site)
        {
            if (ModelState.IsValid)
            {
                bool authorized;
                if (!_sites.GetSites().Any())
                {
                    // provision initial site during installation
                    authorized = true; 
                    Tenant tenant = _tenants.GetTenant();
                    site.TenantId = tenant.TenantId;
                }
                else
                {
                    authorized = User.IsInRole(RoleNames.Host);
                }
                if (authorized)
                {
                    site = _sites.AddSite(site);
                    _logger.Log(site.SiteId, LogLevel.Information, this, LogFunction.Create, "Site Added {Site}", site);
                }
            }
            return site;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public Site Put(int id, [FromBody] Site site)
        {
            if (ModelState.IsValid)
            {
                site = _sites.UpdateSite(site);
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.Site, site.SiteId);
                _logger.Log(site.SiteId, LogLevel.Information, this, LogFunction.Update, "Site Updated {Site}", site);
            }
            return site;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Host)]
        public void Delete(int id)
        {
            _sites.DeleteSite(id);
            _logger.Log(id, LogLevel.Information, this, LogFunction.Delete, "Site Deleted {SiteId}", id);
        }
    }
}

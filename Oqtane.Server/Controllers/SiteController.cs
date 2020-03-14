using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Infrastructure;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SiteController : Controller
    {
        private readonly ISiteRepository _sites;
        private readonly ITenantResolver _tenants;
        private readonly IWebHostEnvironment _environment;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public SiteController(ISiteRepository sites, ITenantResolver tenants, IWebHostEnvironment environment, ISyncManager syncManager, ILogManager logger)
        {
            _sites = sites;
            _tenants = tenants;
            _environment = environment;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        [Authorize(Roles = Constants.HostRole)]
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
        public Site Post([FromBody] Site Site)
        {
            if (ModelState.IsValid)
            {
                bool authorized;
                if (!_sites.GetSites().Any())
                {
                    // provision initial site during installation
                    authorized = true; 
                    Tenant tenant = _tenants.GetTenant();
                    Site.TenantId = tenant.TenantId;
                }
                else
                {
                    authorized = User.IsInRole(Constants.HostRole);
                }
                if (authorized)
                {
                    Site = _sites.AddSite(Site);
                    _logger.Log(Site.SiteId, LogLevel.Information, this, LogFunction.Create, "Site Added {Site}", Site);
                }
            }
            return Site;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public Site Put(int id, [FromBody] Site Site)
        {
            if (ModelState.IsValid)
            {
                Site = _sites.UpdateSite(Site);
                _syncManager.AddSyncEvent(EntityNames.Site, Site.SiteId);
                _logger.Log(Site.SiteId, LogLevel.Information, this, LogFunction.Update, "Site Updated {Site}", Site);
            }
            return Site;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            _sites.DeleteSite(id);
            _logger.Log(id, LogLevel.Information, this, LogFunction.Delete, "Site Deleted {SiteId}", id);
        }
    }
}

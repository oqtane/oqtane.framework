using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SiteController : Controller
    {
        private readonly ISiteRepository Sites;
        private readonly ITenantResolver Tenants;
        private readonly IWebHostEnvironment environment;

        public SiteController(ISiteRepository Sites, ITenantResolver Tenants, IWebHostEnvironment environment)
        {
            this.Sites = Sites;
            this.Tenants = Tenants;
            this.environment = environment;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Site> Get()
        {
            return Sites.GetSites();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Site Get(int id)
        {
            return Sites.GetSite(id);
        }

        // POST api/<controller>
        [HttpPost]
        public Site Post([FromBody] Site Site)
        {
            if (ModelState.IsValid)
            {
                bool authorized;
                if (!Sites.GetSites().Any())
                {
                    authorized = true; // provision initial site during installation
                }
                else
                {
                    authorized = User.IsInRole(Constants.HostRole);
                }
                if (authorized)
                {
                    Site = Sites.AddSite(Site);
                    string folder = environment.WebRootPath + "\\Tenants\\" + Tenants.GetTenant().TenantId.ToString() + "\\Sites\\" + Site.SiteId.ToString();
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
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
                Site = Sites.UpdateSite(Site);
            }
            return Site;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.HostRole)]
        public void Delete(int id)
        {
            Sites.DeleteSite(id);
        }
    }
}

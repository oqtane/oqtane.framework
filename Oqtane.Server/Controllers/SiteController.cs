using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SiteController : Controller
    {
        private readonly ISiteRepository sites;

        public SiteController(ISiteRepository Sites)
        {
            sites = Sites;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Site> Get()
        {
            return sites.GetSites();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Site Get(int id)
        {
            return sites.GetSite(id);
        }

        // POST api/<controller>
        [HttpPost]
        public Site Post([FromBody] Site Site)
        {
            if (ModelState.IsValid)
            {
                Site = sites.AddSite(Site);
            }
            return Site;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public Site Put(int id, [FromBody] Site Site)
        {
            if (ModelState.IsValid)
            {
                Site = sites.UpdateSite(Site);
            }
            return Site;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            sites.DeleteSite(id);
        }
    }
}

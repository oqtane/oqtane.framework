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
        public void Post([FromBody] Site site)
        {
            if (ModelState.IsValid)
                sites.AddSite(site);
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Site site)
        {
            if (ModelState.IsValid)
                sites.UpdateSite(site);
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            sites.DeleteSite(id);
        }
    }
}

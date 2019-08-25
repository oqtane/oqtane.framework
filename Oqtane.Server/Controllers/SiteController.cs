using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SiteController : Controller
    {
        private readonly ISiteRepository Sites;

        public SiteController(ISiteRepository Sites)
        {
            this.Sites = Sites;
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
        [Authorize]
        public Site Post([FromBody] Site Site)
        {
            if (ModelState.IsValid)
            {
                Site = Sites.AddSite(Site);
            }
            return Site;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize]
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
        [Authorize]
        public void Delete(int id)
        {
            Sites.DeleteSite(id);
        }
    }
}

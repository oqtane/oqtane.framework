using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageModuleController : Controller
    {
        private readonly IPageModuleRepository pagemodules;

        public PageModuleController(IPageModuleRepository PageModules)
        {
            pagemodules = PageModules;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<PageModule> Get()
        {
            return pagemodules.GetPageModules();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public PageModule Get(int id)
        {
            return pagemodules.GetPageModule(id);
        }

        // POST api/<controller>
        [HttpPost]
        public PageModule Post([FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid)
            {
                PageModule = pagemodules.AddPageModule(PageModule);
            }
            return PageModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public PageModule Put(int id, [FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid)
            {
                PageModule = pagemodules.UpdatePageModule(PageModule);
            }
            return PageModule;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            pagemodules.DeletePageModule(id);
        }
    }
}

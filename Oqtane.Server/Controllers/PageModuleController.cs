using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageModuleController : Controller
    {
        private readonly IPageModuleRepository PageModules;

        public PageModuleController(IPageModuleRepository PageModules)
        {
            this.PageModules = PageModules;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<PageModule> Get()
        {
            return PageModules.GetPageModules();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public PageModule Get(int id)
        {
            return PageModules.GetPageModule(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        public PageModule Post([FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid)
            {
                PageModule = PageModules.AddPageModule(PageModule);
            }
            return PageModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrators")]
        public PageModule Put(int id, [FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid)
            {
                PageModule = PageModules.UpdatePageModule(PageModule);
            }
            return PageModule;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrators")]
        public void Delete(int id)
        {
            PageModules.DeletePageModule(id);
        }
    }
}

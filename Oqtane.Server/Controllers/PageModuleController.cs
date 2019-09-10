using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageModuleController : Controller
    {
        private readonly IPageModuleRepository PageModules;
        private readonly IModuleRepository Modules;

        public PageModuleController(IPageModuleRepository PageModules, IModuleRepository Modules)
        {
            this.PageModules = PageModules;
            this.Modules = Modules;
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
        [Authorize(Roles = Constants.AdminRole)]
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
        [Authorize(Roles = Constants.AdminRole)]
        public PageModule Put(int id, [FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid)
            {
                PageModule = PageModules.UpdatePageModule(PageModule);
            }
            return PageModule;
        }

        // PUT api/<controller>/?pageid=x&pane=y
        [HttpPut]
        [Authorize(Roles = Constants.AdminRole)]
        public void Put(int pageid, string pane)
        {
            int order = 1;
            List<PageModule> pagemodules = PageModules.GetPageModules(pageid).ToList();
            foreach (PageModule pagemodule in pagemodules.Where(item => item.Pane == pane).OrderBy(item => item.Order))
            {
                if (pagemodule.Order != order)
                {
                    pagemodule.Order = order;
                    PageModules.UpdatePageModule(pagemodule);
                }
                order += 2;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            PageModules.DeletePageModule(id);
        }
    }
}

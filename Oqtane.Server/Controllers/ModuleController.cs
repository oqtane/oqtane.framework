using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class ModuleController : Controller
    {
        private readonly IModuleRepository Modules;
        private readonly IPageModuleRepository PageModules;

        public ModuleController(IModuleRepository Modules, IPageModuleRepository PageModules)
        {
            this.Modules = Modules;
            this.PageModules = PageModules;
        }

        // GET: api/<controller>?pageid=x
        // GET: api/<controller>?siteid=x&moduledefinitionname=x
        [HttpGet]
        public IEnumerable<Module> Get(string pageid, string siteid, string moduledefinitionname)
        {
            if (!string.IsNullOrEmpty(pageid))
            {
                List<Module> modulelist = new List<Module>();
                foreach (PageModule pagemodule in PageModules.GetPageModules(int.Parse(pageid)))
                {
                    Module module = pagemodule.Module;
                    module.PageModuleId = pagemodule.PageModuleId;
                    module.PageId = pagemodule.PageId;
                    module.Title = pagemodule.Title;
                    module.Pane = pagemodule.Pane;
                    module.Order = pagemodule.Order;
                    module.ContainerType = pagemodule.ContainerType;
                    modulelist.Add(module);
                }
                return modulelist;
            }
            else
            {
                return Modules.GetModules(int.Parse(siteid), moduledefinitionname);
            }
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Module Get(int id)
        {
            return Modules.GetModule(id);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.AdminRole)]
        public Module Post([FromBody] Module Module)
        {
            if (ModelState.IsValid)
            {
                Module = Modules.AddModule(Module);
            }
            return Module;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public Module Put(int id, [FromBody] Module Module)
        {
            if (ModelState.IsValid)
            {
                Module = Modules.UpdateModule(Module);
            }
            return Module;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.AdminRole)]
        public void Delete(int id)
        {
            Modules.DeleteModule(id);
        }
    }
}

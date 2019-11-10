using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Infrastructure;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageModuleController : Controller
    {
        private readonly IPageModuleRepository PageModules;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public PageModuleController(IPageModuleRepository PageModules, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.PageModules = PageModules;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
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

        // GET: api/<controller>/pageid/moduleid
        [HttpGet("{pageid}/{moduleid}")]
        public PageModule Get(int pageid, int moduleid)
        {
            return PageModules.GetPageModule(pageid, moduleid);
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public PageModule Post([FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Page", PageModule.PageId, "Edit"))
            {
                PageModule = PageModules.AddPageModule(PageModule);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Module Added {PageModule}", PageModule);
            }
            return PageModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public PageModule Put(int id, [FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Page", PageModule.PageId, "Edit"))
            {
                PageModule = PageModules.UpdatePageModule(PageModule);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Updated {PageModule}", PageModule);
            }
            return PageModule;
        }

        // PUT api/<controller>/?pageid=x&pane=y
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int pageid, string pane)
        {
            if (UserPermissions.IsAuthorized(User, "Page", pageid, "Edit"))
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
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Order Updated {PageId} {Pane}", pageid, pane);
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            PageModule pagemodule = PageModules.GetPageModule(id);
            if (UserPermissions.IsAuthorized(User, "Page", pagemodule.PageId, "Edit"))
            {
                PageModules.DeletePageModule(id);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Module Deleted {PageModuleId}", id);
            }
        }
    }
}

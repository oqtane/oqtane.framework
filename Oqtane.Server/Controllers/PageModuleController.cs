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
        private readonly IModuleRepository Modules;
        private readonly IUserPermissions UserPermissions;
        private readonly ILogManager logger;

        public PageModuleController(IPageModuleRepository PageModules, IModuleRepository Modules, IUserPermissions UserPermissions, ILogManager logger)
        {
            this.PageModules = PageModules;
            this.Modules = Modules;
            this.UserPermissions = UserPermissions;
            this.logger = logger;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public PageModule Get(int id)
        {
            PageModule pagemodule = PageModules.GetPageModule(id);
            if (UserPermissions.IsAuthorized(User, "View", pagemodule.Module.Permissions))
            {
                return pagemodule;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access PageModule {PageModule}", pagemodule);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // GET: api/<controller>/pageid/moduleid
        [HttpGet("{pageid}/{moduleid}")]
        public PageModule Get(int pageid, int moduleid)
        {
            PageModule pagemodule = PageModules.GetPageModule(pageid, moduleid);
            if (UserPermissions.IsAuthorized(User, "View", pagemodule.Module.Permissions))
            {
                return pagemodule;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access PageModule {PageModule}", pagemodule);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
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
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add PageModule {PageModule}", PageModule);
                HttpContext.Response.StatusCode = 401;
                PageModule = null;
            }
            return PageModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public PageModule Put(int id, [FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Module", PageModule.ModuleId, "Edit"))
            {
                PageModule = PageModules.UpdatePageModule(PageModule);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Updated {PageModule}", PageModule);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update PageModule {PageModule}", PageModule);
                HttpContext.Response.StatusCode = 401;
                PageModule = null;
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
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page Module Order {PageId} {Pane}", pageid, pane);
                HttpContext.Response.StatusCode = 401;
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
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete PageModule {PageModuleId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }
    }
}

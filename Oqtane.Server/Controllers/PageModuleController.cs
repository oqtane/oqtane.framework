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
        private readonly IPageModuleRepository _pageModules;
        private readonly IModuleRepository _modules;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public PageModuleController(IPageModuleRepository pageModules, IModuleRepository modules, IUserPermissions userPermissions, ILogManager logger)
        {
            _pageModules = pageModules;
            _modules = modules;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public PageModule Get(int id)
        {
            PageModule pagemodule = _pageModules.GetPageModule(id);
            if (_userPermissions.IsAuthorized(User, "View", pagemodule.Module.Permissions))
            {
                return pagemodule;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access PageModule {PageModule}", pagemodule);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // GET: api/<controller>/pageid/moduleid
        [HttpGet("{pageid}/{moduleid}")]
        public PageModule Get(int pageid, int moduleid)
        {
            PageModule pagemodule = _pageModules.GetPageModule(pageid, moduleid);
            if (_userPermissions.IsAuthorized(User, "View", pagemodule.Module.Permissions))
            {
                return pagemodule;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access PageModule {PageModule}", pagemodule);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public PageModule Post([FromBody] PageModule PageModule)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, "Page", PageModule.PageId, "Edit"))
            {
                PageModule = _pageModules.AddPageModule(PageModule);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Module Added {PageModule}", PageModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add PageModule {PageModule}", PageModule);
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
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, "Module", PageModule.ModuleId, "Edit"))
            {
                PageModule = _pageModules.UpdatePageModule(PageModule);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Updated {PageModule}", PageModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update PageModule {PageModule}", PageModule);
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
            if (_userPermissions.IsAuthorized(User, "Page", pageid, "Edit"))
            {
                int order = 1;
                List<PageModule> pagemodules = _pageModules.GetPageModules(pageid, pane).OrderBy(item => item.Order).ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    if (pagemodule.Order != order)
                    {
                        pagemodule.Order = order;
                        _pageModules.UpdatePageModule(pagemodule);
                    }
                    order += 2;
                }
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Order Updated {PageId} {Pane}", pageid, pane);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page Module Order {PageId} {Pane}", pageid, pane);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            PageModule pagemodule = _pageModules.GetPageModule(id);
            if (_userPermissions.IsAuthorized(User, "Page", pagemodule.PageId, "Edit"))
            {
                _pageModules.DeletePageModule(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Module Deleted {PageModuleId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete PageModule {PageModuleId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }
    }
}

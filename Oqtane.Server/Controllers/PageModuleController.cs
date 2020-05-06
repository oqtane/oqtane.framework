using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Security;

namespace Oqtane.Controllers
{
    [Route("{alias}/api/[controller]")]
    public class PageModuleController : Controller
    {
        private readonly IPageModuleRepository _pageModules;
        private readonly IUserPermissions _userPermissions;
        private readonly ITenantResolver _tenants;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public PageModuleController(IPageModuleRepository pageModules, IUserPermissions userPermissions, ITenantResolver tenants, ISyncManager syncManager, ILogManager logger)
        {
            _pageModules = pageModules;
            _userPermissions = userPermissions;
            _tenants = tenants;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public PageModule Get(int id)
        {
            PageModule pagemodule = _pageModules.GetPageModule(id);
            if (_userPermissions.IsAuthorized(User,PermissionNames.View, pagemodule.Module.Permissions))
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
            if (_userPermissions.IsAuthorized(User,PermissionNames.View, pagemodule.Module.Permissions))
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
        public PageModule Post([FromBody] PageModule pageModule)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Page, pageModule.PageId, PermissionNames.Edit))
            {
                pageModule = _pageModules.AddPageModule(pageModule);
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.Page, pageModule.PageId);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Module Added {PageModule}", pageModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add PageModule {PageModule}", pageModule);
                HttpContext.Response.StatusCode = 401;
                pageModule = null;
            }
            return pageModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public PageModule Put(int id, [FromBody] PageModule pageModule)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Module, pageModule.ModuleId, PermissionNames.Edit))
            {
                pageModule = _pageModules.UpdatePageModule(pageModule);
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.Page, pageModule.PageId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Updated {PageModule}", pageModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update PageModule {PageModule}", pageModule);
                HttpContext.Response.StatusCode = 401;
                pageModule = null;
            }
            return pageModule;
        }

        // PUT api/<controller>/?pageid=x&pane=y
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int pageid, string pane)
        {
            if (_userPermissions.IsAuthorized(User, EntityNames.Page, pageid, PermissionNames.Edit))
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
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.Page, pageid);
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
            if (_userPermissions.IsAuthorized(User, EntityNames.Page, pagemodule.PageId, PermissionNames.Edit))
            {
                _pageModules.DeletePageModule(id);
                _syncManager.AddSyncEvent(_tenants.GetTenant().TenantId, EntityNames.Page, pagemodule.PageId);
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

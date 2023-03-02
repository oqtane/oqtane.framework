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
using System.Net;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class PageModuleController : Controller
    {
        private readonly IPageModuleRepository _pageModules;
        private readonly IPageRepository _pages;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public PageModuleController(IPageModuleRepository pageModules, IPageRepository pages, IUserPermissions userPermissions, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _pageModules = pageModules;
            _pages = pages;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public PageModule Get(int id)
        {
            PageModule pagemodule = _pageModules.GetPageModule(id);
            if (pagemodule != null && pagemodule.Module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, pagemodule.Module.PermissionList))
            {
                return pagemodule;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PageModule Get Attempt {PageModuleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // GET: api/<controller>/pageid/moduleid
        [HttpGet("{pageid}/{moduleid}")]
        public PageModule Get(int pageid, int moduleid)
        {
            PageModule pagemodule = _pageModules.GetPageModule(pageid, moduleid);
            if (pagemodule != null && pagemodule.Module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, pagemodule.Module.PermissionList))
            {
                return pagemodule;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PageModule Get Attempt {PageId} {ModuleId}", pageid, moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public PageModule Post([FromBody] PageModule pageModule)
        {
            var page = _pages.GetPage(pageModule.PageId);
            if (ModelState.IsValid && page != null && page.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, page.SiteId, EntityNames.Page, pageModule.PageId, PermissionNames.Edit))
            {
                pageModule = _pageModules.AddPageModule(pageModule);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.PageModule, pageModule.PageModuleId, SyncEventActions.Create);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Module Added {PageModule}", pageModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PageModule Post Attempt {PageModule}", pageModule);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                pageModule = null;
            }
            return pageModule;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public PageModule Put(int id, [FromBody] PageModule pageModule)
        {
            var page = _pages.GetPage(pageModule.PageId);
            if (ModelState.IsValid && page != null && page.SiteId == _alias.SiteId && _pageModules.GetPageModule(pageModule.PageModuleId, false) != null && _userPermissions.IsAuthorized(User, page.SiteId, EntityNames.Page, pageModule.PageId, PermissionNames.Edit))
            {
                pageModule = _pageModules.UpdatePageModule(pageModule);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.PageModule, pageModule.PageModuleId, SyncEventActions.Update);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Updated {PageModule}", pageModule);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PageModule Put Attempt {PageModule}", pageModule);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                pageModule = null;
            }
            return pageModule;
        }

        // PUT api/<controller>/?pageid=x&pane=y
        [HttpPut]
        [Authorize(Roles = RoleNames.Registered)]
        public void Put(int pageid, string pane)
        {
            var page = _pages.GetPage(pageid);
            if (page != null && page.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, page.SiteId, EntityNames.Page, pageid, PermissionNames.Edit))
            {
                int order = 1;
                List<PageModule> pagemodules = _pageModules.GetPageModules(page.SiteId)
                    .Where(item => item.PageId == pageid && item.Pane == pane).OrderBy(item => item.Order).ToList();
                foreach (PageModule pagemodule in pagemodules)
                {
                    if (pagemodule.Order != order)
                    {
                        pagemodule.Order = order;
                        _pageModules.UpdatePageModule(pagemodule);
                        _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.PageModule, pagemodule.PageModuleId, SyncEventActions.Update);
                    }
                    order += 2;
                }
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Module Order Updated {PageId} {Pane}", pageid, pane);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PageModule Put Attempt {PageId} {Pane}", pageid, pane);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
           }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            PageModule pagemodule = _pageModules.GetPageModule(id);
            if (pagemodule != null && pagemodule.Module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, pagemodule.Module.SiteId, EntityNames.Page, pagemodule.PageId, PermissionNames.Edit))
            {
                _pageModules.DeletePageModule(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.PageModule, pagemodule.PageModuleId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Module Deleted {PageModuleId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized PageModule Delete Attempt {PageModuleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Security;
using System.Net;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageController : Controller
    {
        private readonly IPageRepository _pages;
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pageModules;
        private readonly IUserPermissions _userPermissions;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public PageController(IPageRepository pages, IModuleRepository modules, IPageModuleRepository pageModules, IUserPermissions userPermissions, IPermissionRepository permissionRepository, ISyncManager syncManager, ILogManager logger)
        {
            _pages = pages;
            _modules = modules;
            _pageModules = pageModules;
            _userPermissions = userPermissions;
            _permissionRepository = permissionRepository;
            _syncManager = syncManager;
            _logger = logger;
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Page> Get(string siteid)
        {
            List<Page> pages = new List<Page>();
            foreach (Page page in _pages.GetPages(int.Parse(siteid)))
            {
                if (_userPermissions.IsAuthorized(User,PermissionNames.View, page.Permissions))
                {
                    pages.Add(page);
                }
            }
            return pages;
        }

        // GET api/<controller>/5?userid=x
        [HttpGet("{id}")]
        public Page Get(int id, string userid)
        {
            Page page;
            if (string.IsNullOrEmpty(userid))
            {
                page = _pages.GetPage(id);
            }
            else
            {
                page = _pages.GetPage(id, int.Parse(userid));
            }
            if (_userPermissions.IsAuthorized(User,PermissionNames.View, page.Permissions))
            {
                return page;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Page {Page}", page);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // GET api/<controller>/path/x?path=y
        [HttpGet("path/{siteid}")]
        public Page Get(string path, int siteid)
        {
            Page page = _pages.GetPage(WebUtility.UrlDecode(path), siteid);
            if (page != null)
            {
                if (_userPermissions.IsAuthorized(User,PermissionNames.View, page.Permissions))
                {
                    return page;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Page {Page}", page);
                    HttpContext.Response.StatusCode = 401;
                    return null;
                }
            }
            else
            {
                HttpContext.Response.StatusCode = 404;
                return null;
            }
        }
        
        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Post([FromBody] Page page)
        {
            if (ModelState.IsValid)
            {
                string permissions;
                if (page.ParentId != null)
                {
                    permissions = _pages.GetPage(page.ParentId.Value).Permissions;
                }
                else
                {
                    permissions = _permissionRepository.EncodePermissions(new List<Permission> {
                        new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                    });
                }
            
                if (_userPermissions.IsAuthorized(User,PermissionNames.Edit, permissions))
                {
                    page = _pages.AddPage(page);
                    _syncManager.AddSyncEvent(EntityNames.Site, page.SiteId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Added {Page}", page);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Page {Page}", page);
                    HttpContext.Response.StatusCode = 401;
                    page = null;
                }
            }
            return page;
        }

        // POST api/<controller>/5?userid=x
        [HttpPost("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Post(int id, string userid)
        {
            Page page = null;
            Page parent = _pages.GetPage(id);
            if (parent != null && parent.IsPersonalizable && _userPermissions.GetUser(User).UserId == int.Parse(userid))
            {
                page = new Page();
                page.SiteId = parent.SiteId;
                page.Name = parent.Name;
                page.Title = parent.Title;
                page.Path = parent.Path;
                page.ParentId = parent.PageId;
                page.Order = 0;
                page.IsNavigation = false;
                page.Url = "";
                page.EditMode = false;
                page.ThemeType = parent.ThemeType;
                page.LayoutType = parent.LayoutType;
                page.Icon = parent.Icon;
                page.Permissions = _permissionRepository.EncodePermissions(new List<Permission> {
                    new Permission(PermissionNames.View, userid, true),
                    new Permission(PermissionNames.Edit, userid, true)
                });
                page.IsPersonalizable = false;
                page.UserId = int.Parse(userid);
                page = _pages.AddPage(page);
                _syncManager.AddSyncEvent(EntityNames.Site, page.SiteId);

                // copy modules
                List<PageModule> pagemodules = _pageModules.GetPageModules(page.SiteId).ToList();
                foreach (PageModule pm in pagemodules.Where(item => item.PageId == parent.PageId && !item.IsDeleted))
                {
                    Module module = new Module();
                    module.SiteId = page.SiteId;
                    module.PageId = page.PageId;
                    module.ModuleDefinitionName = pm.Module.ModuleDefinitionName;
                    module.Permissions = _permissionRepository.EncodePermissions(new List<Permission> {
                        new Permission(PermissionNames.View, userid, true),
                        new Permission(PermissionNames.Edit, userid, true)
                    });
                    module = _modules.AddModule(module);

                    string content = _modules.ExportModule(pm.ModuleId);
                    if (content != "")
                    {
                        _modules.ImportModule(module.ModuleId, content);
                    }

                    PageModule pagemodule = new PageModule();
                    pagemodule.PageId = page.PageId;
                    pagemodule.ModuleId = module.ModuleId;
                    pagemodule.Title = pm.Title;
                    pagemodule.Pane = pm.Pane;
                    pagemodule.Order = pm.Order;
                    pagemodule.ContainerType = pm.ContainerType;

                    _pageModules.AddPageModule(pagemodule);
                }
            }
            return page;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Page Put(int id, [FromBody] Page page)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Page, page.PageId, PermissionNames.Edit))
            {
                page = _pages.UpdatePage(page);
                _syncManager.AddSyncEvent(EntityNames.Site, page.SiteId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Updated {Page}", page);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page {Page}", page);
                HttpContext.Response.StatusCode = 401;
                page = null;
            }
            return page;
        }

        // PUT api/<controller>/?siteid=x&pageid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int siteid, int pageid, int? parentid)
        {
            if (_userPermissions.IsAuthorized(User, EntityNames.Page, pageid, PermissionNames.Edit))
            {
                int order = 1;
                List<Page> pages = _pages.GetPages(siteid).ToList();
                foreach (Page page in pages.Where(item => item.ParentId == parentid).OrderBy(item => item.Order))
                {
                    if (page.Order != order)
                    {
                        page.Order = order;
                        _pages.UpdatePage(page);
                    }
                    order += 2;
                }
                _syncManager.AddSyncEvent(EntityNames.Site, siteid);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Order Updated {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page Order {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            Page page = _pages.GetPage(id);
            if (_userPermissions.IsAuthorized(User, EntityNames.Page, page.PageId, PermissionNames.Edit))
            {
                _pages.DeletePage(page.PageId);
                _syncManager.AddSyncEvent(EntityNames.Site, page.SiteId);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Deleted {PageId}", page.PageId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Page {PageId}", page.PageId);
                HttpContext.Response.StatusCode = 401;
            }
        }
    }
}

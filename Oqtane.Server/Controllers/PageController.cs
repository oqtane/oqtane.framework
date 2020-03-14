using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Infrastructure;
using Oqtane.Security;
using System.Net;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class PageController : Controller
    {
        private readonly IPageRepository _pages;
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pageModules;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;

        public PageController(IPageRepository pages, IModuleRepository modules, IPageModuleRepository pageModules, IUserPermissions userPermissions, ISyncManager syncManager, ILogManager logger)
        {
            _pages = pages;
            _modules = modules;
            _pageModules = pageModules;
            _userPermissions = userPermissions;
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
        public Page Post([FromBody] Page Page)
        {
            if (ModelState.IsValid)
            {
                string permissions;
                if (Page.ParentId != null)
                {
                    permissions = _pages.GetPage(Page.ParentId.Value).Permissions;
                }
                else
                {
                    permissions = UserSecurity.SetPermissionStrings(new List<PermissionString> { new PermissionString { PermissionName = "Edit", Permissions = Constants.AdminRole } });
                }
            
                if (_userPermissions.IsAuthorized(User,PermissionNames.Edit, permissions))
                {
                    Page = _pages.AddPage(Page);
                    _syncManager.AddSyncEvent("Site", Page.SiteId);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Added {Page}", Page);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Page {Page}", Page);
                    HttpContext.Response.StatusCode = 401;
                    Page = null;
                }
            }
            return Page;
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
                page.Path = parent.Path;
                page.ParentId = parent.PageId;
                page.Order = 0;
                page.IsNavigation = false;
                page.EditMode = false;
                page.ThemeType = parent.ThemeType;
                page.LayoutType = parent.LayoutType;
                page.Icon = parent.Icon;
                List<PermissionString> permissions = new List<PermissionString>();
                permissions.Add(new PermissionString { PermissionName = "View", Permissions = "[" + userid + "]" });
                permissions.Add(new PermissionString { PermissionName = "Edit", Permissions = "[" + userid + "]" });
                page.Permissions = UserSecurity.SetPermissionStrings(permissions);
                page.IsPersonalizable = false;
                page.UserId = int.Parse(userid);
                page = _pages.AddPage(page);
                _syncManager.AddSyncEvent("Site", page.SiteId);

                // copy modules
                List<PageModule> pagemodules = _pageModules.GetPageModules(page.SiteId).ToList();
                foreach (PageModule pm in pagemodules.Where(item => item.PageId == parent.PageId && !item.IsDeleted))
                {
                    Module module = new Module();
                    module.SiteId = page.SiteId;
                    module.PageId = page.PageId;
                    module.ModuleDefinitionName = pm.Module.ModuleDefinitionName;
                    permissions = new List<PermissionString>();
                    permissions.Add(new PermissionString { PermissionName = "View", Permissions = "[" + userid + "]" });
                    permissions.Add(new PermissionString { PermissionName = "Edit", Permissions = "[" + userid + "]" });
                    module.Permissions = UserSecurity.SetPermissionStrings(permissions);
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
        public Page Put(int id, [FromBody] Page Page)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, "Page", Page.PageId, "Edit"))
            {
                Page = _pages.UpdatePage(Page);
                _syncManager.AddSyncEvent("Site", Page.SiteId);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Updated {Page}", Page);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Page {Page}", Page);
                HttpContext.Response.StatusCode = 401;
                Page = null;
            }
            return Page;
        }

        // PUT api/<controller>/?siteid=x&pageid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Put(int siteid, int pageid, int? parentid)
        {
            if (_userPermissions.IsAuthorized(User, "Page", pageid, "Edit"))
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
                _syncManager.AddSyncEvent("Site", siteid);
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
            if (_userPermissions.IsAuthorized(User, "Page", page.PageId, "Edit"))
            {
                _pages.DeletePage(page.PageId);
                _syncManager.AddSyncEvent("Site", page.SiteId);
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

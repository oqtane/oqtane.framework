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
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class PageController : Controller
    {
        private readonly IPageRepository _pages;
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pageModules;
        private readonly IPermissionRepository _permissionRepository;
        private readonly ISettingRepository _settings;
        private readonly IUserPermissions _userPermissions;
        private readonly IUrlMappingRepository _urlMappings;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public PageController(IPageRepository pages, IModuleRepository modules, IPageModuleRepository pageModules, IPermissionRepository permissionRepository, ISettingRepository settings, IUserPermissions userPermissions, IUrlMappingRepository urlMappings, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _pages = pages;
            _modules = modules;
            _pageModules = pageModules;
            _permissionRepository = permissionRepository;
            _settings = settings;
            _userPermissions = userPermissions;
            _urlMappings = urlMappings;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Page> Get(string siteid, string includedeleted)
        {
            List<Page> pages = new List<Page>();

            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                var includeDeleted = bool.Parse(includedeleted);
                List<Setting> settings = _settings.GetSettings(EntityNames.Page).ToList();

                foreach (Page page in _pages.GetPages(SiteId))
                {
                    if ((includeDeleted || !page.IsDeleted) && _userPermissions.IsAuthorized(User, PermissionNames.View, page.PermissionList))
                    {
                        page.Settings = settings.Where(item => item.EntityId == page.PageId)
                            .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, page.PermissionList))
                            .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                        pages.Add(page);
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Page Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                pages = null;
            }

            return pages;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Page Get(int id)
        {
            var page = _pages.GetPage(id);
            if (page != null && page.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, page.PermissionList))
            {
                page.Settings = _settings.GetSettings(EntityNames.Page, page.PageId)
                    .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, page.PermissionList))
                    .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                return page;
            }
            else
            {
                if (page != null)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Security, "Unauthorized Page Get Attempt {PageId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        // GET api/<controller>/path/x?path=y
        [HttpGet("path/{siteid}")]
        public Page Get(string path, int siteid)
        {
            Page page = _pages.GetPage(WebUtility.UrlDecode(path), siteid);
            if (page != null && page.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, page.PermissionList))
            {
                page.Settings = _settings.GetSettings(EntityNames.Page, page.PageId)
                    .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, page.PermissionList))
                    .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                return page;
            }
            else
            {
                if (page != null)
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Security, "Unauthorized Page Get Attempt {SiteId} {Path}", siteid, path);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public Page Post([FromBody] Page page)
        {
            if (ModelState.IsValid && page.SiteId == _alias.SiteId)
            {
                List<Permission> permissions;
                if (page.ParentId != null)
                {
                    permissions = _pages.GetPage(page.ParentId.Value).PermissionList;
                }
                else
                {
                    permissions = new List<Permission> {
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    };
                }

                if (_userPermissions.IsAuthorized(User, PermissionNames.Edit, permissions))
                {
                    page = _pages.AddPage(page);
                    _syncManager.AddSyncEvent(_alias, EntityNames.Page, page.PageId, SyncEventActions.Create);
                    _syncManager.AddSyncEvent(_alias, EntityNames.Site, page.SiteId, SyncEventActions.Refresh);
                    _logger.Log(LogLevel.Information, this, LogFunction.Create, "Page Added {Page}", page);

                    if (!page.Path.StartsWith("admin/"))
                    {
                        var modules = _modules.GetModules(page.SiteId).Where(item => item.AllPages).ToList();
                        foreach (Module module in modules)
                        {
                            var pageModule = _pageModules.GetPageModules(page.SiteId).FirstOrDefault(item => item.ModuleId == module.ModuleId);
                            _pageModules.AddPageModule(new PageModule { PageId = page.PageId, ModuleId = pageModule.ModuleId, Title = pageModule.Title, Pane = pageModule.Pane, Order = pageModule.Order, ContainerType = pageModule.ContainerType });
                        }
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Create, "User Not Authorized To Add Page {Page}", page);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    page = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Page Post Attempt {Page}", page);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                page = null;
            }
            return page;
        }

        // POST api/<controller>/5?userid=x
        [HttpPost("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Page Post(int id, string userid)
        {
            Page page = null;
            Page parent = _pages.GetPage(id);
            User user = _userPermissions.GetUser(User);
            if (parent != null && parent.SiteId == _alias.SiteId && parent.IsPersonalizable && user.UserId == int.Parse(userid))
            {
                page = _pages.GetPage(parent.Path + "/" + user.Username, parent.SiteId);
                if (page == null)
                {
                    page = new Page();
                    page.SiteId = parent.SiteId;
                    page.ParentId = parent.PageId;
                    page.Name = (!string.IsNullOrEmpty(user.DisplayName)) ? user.DisplayName : user.Username;
                    page.Path = parent.Path + "/" + user.Username;
                    page.Title = page.Name + " - " + parent.Name;
                    page.Order = 0;
                    page.IsNavigation = false;
                    page.Url = "";
                    page.ThemeType = parent.ThemeType;
                    page.DefaultContainerType = parent.DefaultContainerType;
                    page.Icon = parent.Icon;
                    page.PermissionList = new List<Permission>()
                    {
                        new Permission(PermissionNames.View, int.Parse(userid), true),
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.Edit, int.Parse(userid), true)
                    };
                    page.IsPersonalizable = false;
                    page.UserId = int.Parse(userid);
                    page = _pages.AddPage(page);

                    // copy modules
                    List<PageModule> pagemodules = _pageModules.GetPageModules(page.SiteId).ToList();
                    foreach (PageModule pm in pagemodules.Where(item => item.PageId == parent.PageId && !item.IsDeleted))
                    {
                        Module module = new Module();
                        module.SiteId = page.SiteId;
                        module.PageId = page.PageId;
                        module.ModuleDefinitionName = pm.Module.ModuleDefinitionName;
                        module.AllPages = false;
                        module.PermissionList = new List<Permission>()
                        {
                            new Permission(PermissionNames.View, int.Parse(userid), true),
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.Edit, int.Parse(userid), true)
                        };
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

                    _syncManager.AddSyncEvent(_alias, EntityNames.Page, page.PageId, SyncEventActions.Create);
                    _syncManager.AddSyncEvent(_alias, EntityNames.Site, page.SiteId, SyncEventActions.Refresh);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Page Post Attempt {PageId} By User {UserId}", id, userid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                page = null;
            }
            return page;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Page Put(int id, [FromBody] Page page)
        {
            // get current page
            var currentPage = _pages.GetPage(page.PageId, false);

            if (ModelState.IsValid && page.SiteId == _alias.SiteId && page.PageId == id && currentPage != null && _userPermissions.IsAuthorized(User, page.SiteId, EntityNames.Page, page.PageId, PermissionNames.Edit))
            {
                // get current page permissions
                var currentPermissions = _permissionRepository.GetPermissions(page.SiteId, EntityNames.Page, page.PageId).ToList();

                page = _pages.UpdatePage(page);

                // save url mapping if page path changed
                if (currentPage.Path != page.Path)
                {
                    var urlMapping = _urlMappings.GetUrlMapping(page.SiteId, currentPage.Path);
                    if (urlMapping != null)
                    {
                        urlMapping.MappedUrl = page.Path;
                        _urlMappings.UpdateUrlMapping(urlMapping);
                    }
                }

                // get differences between current and new page permissions
                var added = GetPermissionsDifferences(page.PermissionList, currentPermissions);
                var removed = GetPermissionsDifferences(currentPermissions, page.PermissionList);

                // synchronize module permissions
                if (added.Count > 0 || removed.Count > 0)
                {
                    foreach (PageModule pageModule in _pageModules.GetPageModules(page.SiteId).Where(item => item.PageId == page.PageId).ToList())
                    {
                        var modulePermissions = _permissionRepository.GetPermissions(pageModule.Module.SiteId, EntityNames.Module, pageModule.Module.ModuleId).ToList();
                        // permissions added
                        foreach (Permission permission in added)
                        {
                            if (!modulePermissions.Any(item => item.PermissionName == permission.PermissionName
                              && item.RoleId == permission.RoleId && item.UserId == permission.UserId && item.IsAuthorized == permission.IsAuthorized))
                            {
                                _permissionRepository.AddPermission(new Permission
                                {
                                    SiteId = page.SiteId,
                                    EntityName = EntityNames.Module,
                                    EntityId = pageModule.ModuleId,
                                    PermissionName = permission.PermissionName,
                                    RoleId = permission.RoleId,
                                    UserId = permission.UserId,
                                    IsAuthorized = permission.IsAuthorized
                                });
                            }
                        }
                        // permissions removed
                        foreach (Permission permission in removed)
                        {
                            var modulePermission = modulePermissions.FirstOrDefault(item => item.PermissionName == permission.PermissionName
                              && item.RoleId == permission.RoleId && item.UserId == permission.UserId && item.IsAuthorized == permission.IsAuthorized);
                            if (modulePermission != null)
                            {
                                _permissionRepository.DeletePermission(modulePermission.PermissionId);
                            }
                        }
                    }
                }

                // update child paths
                if (page.ParentId != currentPage.ParentId)
                {
                    foreach (Page _page in _pages.GetPages(page.SiteId).Where(item => item.Path.StartsWith(currentPage.Path)).ToList())
                    {
                        _page.Path = _page.Path.Replace(currentPage.Path, page.Path);
                        _pages.UpdatePage(_page);
                    }
                }

                _syncManager.AddSyncEvent(_alias, EntityNames.Page, page.PageId, SyncEventActions.Update);
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, page.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Updated {Page}", page);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Page Put Attempt {Page}", page);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                page = null;
            }
            return page;
        }

        private List<Permission> GetPermissionsDifferences(List<Permission> permissions1, List<Permission> permissions2)
        {
            var differences = new List<Permission>();
            foreach (Permission p in permissions1)
            {
                if (!permissions2.Any(item => item.PermissionName == p.PermissionName && item.RoleId == p.RoleId && item.UserId == p.UserId && item.IsAuthorized == p.IsAuthorized))
                {
                    differences.Add(p);
                }
            }
            return differences;
        }

        // PUT api/<controller>/?siteid=x&pageid=y&parentid=z
        [HttpPut]
        [Authorize(Roles = RoleNames.Registered)]
        public void Put(int siteid, int pageid, int? parentid)
        {
            if (siteid == _alias.SiteId && _pages.GetPage(pageid, false) != null && _userPermissions.IsAuthorized(User, siteid, EntityNames.Page, pageid, PermissionNames.Edit))
            {
                int order = 1;
                List<Page> pages = _pages.GetPages(siteid).ToList();
                foreach (Page page in pages.Where(item => item.ParentId == parentid).OrderBy(item => item.Order))
                {
                    if (page.Order != order)
                    {
                        page.Order = order;
                        _pages.UpdatePage(page);
                        _syncManager.AddSyncEvent(_alias, EntityNames.Page, page.PageId, SyncEventActions.Update);
                    }
                    order += 2;
                }

                _syncManager.AddSyncEvent(_alias, EntityNames.Site, siteid, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Page Order Updated {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Page Put Attempt {SiteId} {PageId} {ParentId}", siteid, pageid, parentid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            Page page = _pages.GetPage(id);
            if (page != null && page.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, page.SiteId, EntityNames.Page, page.PageId, PermissionNames.Edit))
            {
                _pages.DeletePage(page.PageId);
                _syncManager.AddSyncEvent(_alias, EntityNames.Page, page.PageId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, page.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Page Deleted {PageId}", page.PageId);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Page Delete Attempt {PageId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }

}

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
using System.Security.Policy;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class ModuleController : Controller
    {
        private readonly IModuleRepository _modules;
        private readonly IPageModuleRepository _pageModules;
        private readonly IPageRepository _pages;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly ISettingRepository _settings;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ModuleController(IModuleRepository modules, IPageModuleRepository pageModules, IPageRepository pages, IModuleDefinitionRepository moduleDefinitions, ISettingRepository settings, IUserPermissions userPermissions, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _modules = modules; 
            _pageModules = pageModules;
            _pages = pages;
            _moduleDefinitions = moduleDefinitions;
            _settings = settings;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?siteid=x
        [HttpGet]
        public IEnumerable<Module> Get(string siteid)
        {
            List<Module> modules = new List<Module>();

            int SiteId;
            if (int.TryParse(siteid, out SiteId) && SiteId == _alias.SiteId)
            {
                List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(SiteId).ToList();
                List<Setting> settings = _settings.GetSettings(EntityNames.Module).ToList();

                foreach (PageModule pagemodule in _pageModules.GetPageModules(SiteId))
                {
                    if (_userPermissions.IsAuthorized(User, PermissionNames.View, pagemodule.Module.PermissionList))
                    {
                        Module module = new Module();
                        module.SiteId = pagemodule.Module.SiteId;
                        module.ModuleDefinitionName = pagemodule.Module.ModuleDefinitionName;
                        module.AllPages = pagemodule.Module.AllPages;
                        module.PermissionList = pagemodule.Module.PermissionList;
                        module.CreatedBy = pagemodule.Module.CreatedBy;
                        module.CreatedOn = pagemodule.Module.CreatedOn;
                        module.ModifiedBy = pagemodule.Module.ModifiedBy;
                        module.ModifiedOn = pagemodule.Module.ModifiedOn;
                        module.DeletedBy = pagemodule.DeletedBy;
                        module.DeletedOn = pagemodule.DeletedOn;
                        module.IsDeleted = pagemodule.IsDeleted;

                        module.PageModuleId = pagemodule.PageModuleId;
                        module.ModuleId = pagemodule.ModuleId;
                        module.PageId = pagemodule.PageId;
                        module.Title = pagemodule.Title;
                        module.Pane = pagemodule.Pane;
                        module.Order = pagemodule.Order;
                        module.ContainerType = pagemodule.ContainerType;

                        module.ModuleDefinition = _moduleDefinitions.FilterModuleDefinition(moduledefinitions.Find(item => item.ModuleDefinitionName == module.ModuleDefinitionName));

                        module.Settings = settings.Where(item => item.EntityId == pagemodule.ModuleId)
                            .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, pagemodule.Module.PermissionList))
                            .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);

                        modules.Add(module);
                    }
                    modules = modules.OrderBy(item => item.PageId).ThenBy(item => item.Pane).ThenBy(item => item.Order).ToList();
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Get Attempt {SiteId}", siteid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                modules = null;
            }

            return modules;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Module Get(int id)
        {
            Module module = _modules.GetModule(id);
            if (module != null && module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User,PermissionNames.View, module.PermissionList))
            {
                List<ModuleDefinition> moduledefinitions = _moduleDefinitions.GetModuleDefinitions(module.SiteId).ToList();
                module.ModuleDefinition = _moduleDefinitions.FilterModuleDefinition(moduledefinitions.Find(item => item.ModuleDefinitionName == module.ModuleDefinitionName));
                module.Settings = _settings.GetSettings(EntityNames.Module, id)
                    .Where(item => !item.IsPrivate || _userPermissions.IsAuthorized(User, PermissionNames.Edit, module.PermissionList))
                    .ToDictionary(setting => setting.SettingName, setting => setting.SettingValue);
                return module;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Get Attempt {ModuleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public Module Post([FromBody] Module module)
        {
            if (ModelState.IsValid && module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, module.SiteId, EntityNames.Page, module.PageId, PermissionNames.Edit))
            {
                module = _modules.AddModule(module);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Module, module.ModuleId, SyncEventActions.Create);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Module Added {Module}", module);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Post Attempt {Module}", module);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                module = null;
            }
            return module;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Module Put(int id, [FromBody] Module module)
        {
            var _module = _modules.GetModule(module.ModuleId, false);

            if (ModelState.IsValid && module.SiteId == _alias.SiteId && _module != null && _userPermissions.IsAuthorized(User, module.SiteId, EntityNames.Module, module.ModuleId, PermissionNames.Edit))
            {
                module = _modules.UpdateModule(module);

                if (_module.AllPages != module.AllPages)
                {
                    var pageModules = _pageModules.GetPageModules(module.SiteId).ToList();
                    if (module.AllPages)
                    {
                        var pageModule = _pageModules.GetPageModule(module.PageModuleId);
                        var pages = _pages.GetPages(module.SiteId).ToList();
                        foreach (Page page in pages)
                        {
                            if (!pageModules.Exists(item => item.ModuleId == module.ModuleId && item.PageId == page.PageId) && !page.Path.StartsWith("admin/"))
                            {
                                _pageModules.AddPageModule(new PageModule { PageId = page.PageId, ModuleId = pageModule.ModuleId, Title = pageModule.Title, Pane = pageModule.Pane, Order = pageModule.Order, ContainerType = pageModule.ContainerType });
                            }
                        }
                    }
                    else
                    {
                        foreach (var pageModule in pageModules)
                        {
                            if (pageModule.ModuleId == module.ModuleId && pageModule.PageModuleId != module.PageModuleId)
                            {
                                _pageModules.DeletePageModule(pageModule.PageModuleId);
                            }
                        }
                    }
                }

                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Module, module.ModuleId, SyncEventActions.Update);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Updated {Module}", module);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Put Attempt {Module}", module);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                module = null;
            }
            return module;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            var module = _modules.GetModule(id);
            if (module != null && module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, module.SiteId, EntityNames.Module, module.ModuleId, PermissionNames.Edit))
            {
                _modules.DeleteModule(id);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Module, module.ModuleId, SyncEventActions.Delete);
                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Module Deleted {ModuleId}", id);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Delete Attempt {ModuleId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET api/<controller>/export?moduleid=x&pageid=y
        [HttpGet("export")]
        [Authorize(Roles = RoleNames.Registered)]
        public string Export(int moduleid, int pageid)
        {
            string content = "";
            var module = _modules.GetModule(moduleid);
            if (module != null && module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, module.SiteId, EntityNames.Page, pageid, PermissionNames.Edit))
            {
                content = _modules.ExportModule(moduleid);
                if (!string.IsNullOrEmpty(content))
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Read, "Module Content Exported {ModuleId}", moduleid);
                }
                else
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Read, "No Module Content Exported {ModuleId}", moduleid);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Export Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            return content;
        }

        // POST api/<controller>/import?moduleid=x&pageid=y
        [HttpPost("import")]
        [Authorize(Roles = RoleNames.Registered)]
        public bool Import(int moduleid, int pageid, [FromBody] string content)
        {
            bool success = false;
            var module = _modules.GetModule(moduleid);
            if (ModelState.IsValid && module != null && module.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, module.SiteId, EntityNames.Page, pageid, PermissionNames.Edit))
            {
                success = _modules.ImportModule(moduleid, content);
                if (success)
                {
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Module Content Imported {ModuleId}", moduleid);
                }
                else
                {
                    _logger.Log(LogLevel.Warning, this, LogFunction.Update, "Module Content Import Failed {ModuleId}", moduleid);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Module Import Attempt {ModuleId}", moduleid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            return success;
        }
    }
}

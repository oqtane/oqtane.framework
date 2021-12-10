using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Security;
using System.Linq;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using System.Net;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SettingController : Controller
    {
        private readonly ISettingRepository _settings;
        private readonly IPageModuleRepository _pageModules;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SettingController(ISettingRepository settings, IPageModuleRepository pageModules, IUserPermissions userPermissions, ITenantManager tenantManager, ISyncManager syncManager, ILogManager logger)
        {
            _settings = settings;
            _pageModules = pageModules;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Setting> Get(string entityName, int entityid)
        {
            List<Setting> settings = new List<Setting>();
            if (IsAuthorized(entityName, entityid, PermissionNames.View))
            {
                settings = _settings.GetSettings(entityName, entityid).ToList();

                // ispublic filter 
                switch (entityName)
                {
                    case EntityNames.Tenant:
                    case EntityNames.ModuleDefinition:
                    case EntityNames.Host:
                        if (!User.IsInRole(RoleNames.Host))
                        {
                            settings = settings.Where(item => item.IsPublic).ToList();
                        }
                        break;
                    case EntityNames.Site:
                        if (!User.IsInRole(RoleNames.Admin))
                        {
                            settings = settings.Where(item => item.IsPublic).ToList();
                        }
                        break;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Settings {EntityName} {EntityId}", entityName, entityid);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            return settings;
        }

        // GET api/<controller>/5/xxx
        [HttpGet("{id}/{entityName}")]
        public Setting Get(int id, string entityName)
        {
            Setting setting = _settings.GetSetting(entityName, id);
            if (IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.View))
            {
                // ispublic filter
                switch (entityName)
                {
                    case EntityNames.Tenant:
                    case EntityNames.ModuleDefinition:
                    case EntityNames.Host:
                        if (!User.IsInRole(RoleNames.Host) && !setting.IsPublic)
                        {
                            setting = null;
                        }
                        break;
                    case EntityNames.Site:
                        if (!User.IsInRole(RoleNames.Admin) && !setting.IsPublic)
                        {
                            setting = null;
                        }
                        break;
                }

                return setting;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Setting {Setting}", setting);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        public Setting Post([FromBody] Setting setting)
        {
            if (ModelState.IsValid && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                setting = _settings.AddSetting(setting);
                AddSyncEvent(setting.EntityName);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Setting Added {Setting}", setting);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Setting {Setting}", setting);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                setting = null;
            }
            return setting;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public Setting Put(int id, [FromBody] Setting setting)
        {
            if (ModelState.IsValid && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                setting = _settings.UpdateSetting(setting);
                AddSyncEvent(setting.EntityName);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Updated {Setting}", setting);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Setting {Setting}", setting);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                setting = null;
            }
            return setting;
        }

        // DELETE api/<controller>/5/xxx
        [HttpDelete("{id}/{entityName}")]
        public void Delete(string entityName, int id)
        {
            Setting setting = _settings.GetSetting(entityName, id);
            if (IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                _settings.DeleteSetting(setting.EntityName, id);
                AddSyncEvent(setting.EntityName);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Setting Deleted {Setting}", setting);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Setting {Setting}", setting);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private bool IsAuthorized(string entityName, int entityId, string permissionName)
        {
            bool authorized = false;
            if (entityName == EntityNames.PageModule)
            {
                entityName = EntityNames.Module;
                entityId = _pageModules.GetPageModule(entityId).ModuleId;
            }
            switch (entityName)
            {
                case EntityNames.Tenant:
                case EntityNames.ModuleDefinition:
                case EntityNames.Host:
                    if (permissionName == PermissionNames.Edit)
                    {
                        authorized = User.IsInRole(RoleNames.Host);
                    }
                    else
                    {
                        authorized = true;
                    }
                    break;
                case EntityNames.Site:
                    if (permissionName == PermissionNames.Edit)
                    {
                        authorized = User.IsInRole(RoleNames.Admin);
                    }
                    else
                    {
                        authorized = true;
                    }
                    break;
                case EntityNames.Page:
                case EntityNames.Module:
                case EntityNames.Folder:
                    authorized = _userPermissions.IsAuthorized(User, entityName, entityId, permissionName);
                    break;
                case EntityNames.User:
                    authorized = true;
                    if (permissionName == PermissionNames.Edit)
                    {
                        authorized = User.IsInRole(RoleNames.Admin) || (_userPermissions.GetUser(User).UserId == entityId);
                    }
                    break;
                case EntityNames.Visitor:
                    authorized = false;
                    var visitorCookie = "APP_VISITOR_" + _alias.SiteId.ToString();
                    if (int.TryParse(Request.Cookies[visitorCookie], out int visitorId))
                    {
                        authorized = (visitorId == entityId);
                    }
                    break;
            }
            return authorized;
        }

        private void AddSyncEvent(string EntityName)
        {
            switch (EntityName)
            {
                case EntityNames.Module:
                case EntityNames.Page:
                case EntityNames.Site:
                    _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.Site, _alias.SiteId);
                    break;
            }
        }
    }
}

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.RegularExpressions;
using Oqtane.Migrations.Tenant;
using Google.Protobuf.WellKnownTypes;
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class SettingController : Controller
    {
        private readonly ISettingRepository _settings;
        private readonly IPageModuleRepository _pageModules;
        private readonly IUserPermissions _userPermissions;
        private readonly ISyncManager _syncManager;

        private readonly IOptions<CookieAuthenticationOptions> _cookieOptions;
        private readonly IOptionsSnapshot<CookieAuthenticationOptions> _cookieOptionsSnapshot;
        private readonly IOptionsMonitorCache<CookieAuthenticationOptions> _cookieOptionsMonitorCache;

        private readonly IOptions<OpenIdConnectOptions> _oidcOptions;
        private readonly IOptionsSnapshot<OpenIdConnectOptions> _oidcOptionsSnapshot;
        private readonly IOptionsMonitorCache<OpenIdConnectOptions> _oidcOptionsMonitorCache;

        private readonly IOptions<OAuthOptions> _oauthOptions;
        private readonly IOptionsSnapshot<OAuthOptions> _oauthOptionsSnapshot;
        private readonly IOptionsMonitorCache<OAuthOptions> _oauthOptionsMonitorCache;

        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IOptionsSnapshot<IdentityOptions> _identityOptionsSnapshot;
        private readonly IOptionsMonitorCache<IdentityOptions> _identityOptionsMonitorCache;

        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public SettingController(ISettingRepository settings, IPageModuleRepository pageModules, IUserPermissions userPermissions, ITenantManager tenantManager, ISyncManager syncManager, 
            IOptions<CookieAuthenticationOptions> cookieOptions, IOptionsSnapshot<CookieAuthenticationOptions> cookieOptionsSnapshot, IOptionsMonitorCache<CookieAuthenticationOptions> cookieOptionsMonitorCache,
            IOptions<OpenIdConnectOptions> oidcOptions, IOptionsSnapshot<OpenIdConnectOptions> oidcOptionsSnapshot, IOptionsMonitorCache<OpenIdConnectOptions> oidcOptionsMonitorCache,
            IOptions<OAuthOptions> oauthOptions, IOptionsSnapshot<OAuthOptions> oauthOptionsSnapshot, IOptionsMonitorCache<OAuthOptions> oauthOptionsMonitorCache,
            IOptions<IdentityOptions> identityOptions, IOptionsSnapshot<IdentityOptions> identityOptionsSnapshot, IOptionsMonitorCache<IdentityOptions> identityOptionsMonitorCache, 
            ILogManager logger)
        {
            _settings = settings;
            _pageModules = pageModules;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _cookieOptions = cookieOptions;
            _cookieOptionsSnapshot = cookieOptionsSnapshot;
            _cookieOptionsMonitorCache = cookieOptionsMonitorCache;
            _oidcOptions = oidcOptions;
            _oidcOptionsSnapshot = oidcOptionsSnapshot;
            _oidcOptionsMonitorCache = oidcOptionsMonitorCache;
            _oauthOptions = oauthOptions;
            _oauthOptionsSnapshot = oauthOptionsSnapshot;
            _oauthOptionsMonitorCache = oauthOptionsMonitorCache;
            _identityOptions = identityOptions;
            _identityOptionsSnapshot = identityOptionsSnapshot;
            _identityOptionsMonitorCache = identityOptionsMonitorCache;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Setting> Get(string entityName, int entityId)
        {
            List<Setting> settings = new List<Setting>();
            if (IsAuthorized(entityName, entityId, PermissionNames.View))
            {
                settings = _settings.GetSettings(entityName, entityId).ToList();
                if (FilterPrivate(entityName, entityId))
                {
                    settings = settings.Where(item => !item.IsPrivate).ToList();
                }
            }
            else
            {
                // suppress unauthorized visitor logging as it is usually caused by clients that do not support cookies or private browsing sessions
                if (entityName != EntityNames.Visitor) 
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Settings For EntityName {EntityName} And EntityId {EntityId}", entityName, entityId);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
            return settings;
        }

        // GET api/<controller>/5/xxx
        [HttpGet("{id}/{entityName}")]
        public Setting Get(int id, string entityName)
        {
            Setting setting = _settings.GetSetting(entityName, id);
            if (setting != null && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.View))
            {
                if (FilterPrivate(entityName, id) && setting.IsPrivate)
                {
                    setting = null;
                }
                return setting;
            }
            else
            {
                if (setting != null && entityName != EntityNames.Visitor)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access SettingId {SettingId} For EntityName {EntityName} ", id, entityName);
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
        public Setting Post([FromBody] Setting setting)
        {
            if (ModelState.IsValid && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                setting = _settings.AddSetting(setting);
                AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Create);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Setting Added {Setting}", setting);
            }
            else
            {
                if (setting.EntityName != EntityNames.Visitor)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Setting {Setting}", setting);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                setting = null;
            }
            return setting;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public Setting Put(int id, [FromBody] Setting setting)
        {
            if (ModelState.IsValid && setting.SettingId == id && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                setting = _settings.UpdateSetting(setting);
                AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Update);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Updated {Setting}", setting);
            }
            else
            {
                if (setting.EntityName != EntityNames.Visitor)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Setting {Setting}", setting);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                setting = null;
            }
            return setting;
        }

        // PUT api/<controller>/site/1/settingname/x/false
        [HttpPut("{entityName}/{entityId}/{settingName}/{settingValue}/{isPrivate}")]
        public void Put(string entityName, int entityId, string settingName, string settingValue, bool isPrivate)
        {
            if (IsAuthorized(entityName, entityId, PermissionNames.Edit))
            {
                Setting setting = _settings.GetSetting(entityName, entityId, settingName);
                if (setting == null)
                {
                    setting = new Setting();
                    setting.EntityName = entityName;
                    setting.EntityId = entityId;
                    setting.SettingName = settingName;
                    setting.SettingValue = settingValue;
                    setting.IsPrivate = isPrivate;
                    setting = _settings.AddSetting(setting);
                    AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Create);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Created {Setting}", setting);
                }
                else
                {
                    if (setting.SettingValue != settingValue || setting.IsPrivate != isPrivate)
                    {
                        setting.SettingValue = settingValue;
                        setting.IsPrivate = isPrivate;
                        setting = _settings.UpdateSetting(setting);
                        AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Update);
                        _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Updated {Setting}", setting);
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Add Or Update Setting For {EntityName}:{EntityId}", entityName, entityId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // PUT api/<controller>/site/1
        [HttpPut("{entityName}/{entityId}")]
        public void Put(string entityName, int entityId, [FromBody] List<Setting> settings)
        {
            if (ModelState.IsValid && IsAuthorized(entityName,entityId, PermissionNames.Edit))
            {
                var existingSettings = _settings.GetSettings(entityName, entityId).ToList();
                foreach (Setting setting in settings)
                {
                    bool modified = false;

                    // manage settings modified with SetSetting method
                    if (setting.SettingValue.StartsWith("[Private]"))
                    {
                        modified = true;
                        setting.IsPrivate = true;
                        setting.SettingValue = setting.SettingValue.Substring(9);
                    }
                    if (setting.SettingValue.StartsWith("[Public]"))
                    {
                        modified = true;
                        setting.IsPrivate = false;
                        setting.SettingValue = setting.SettingValue.Substring(8);
                    }

                    Setting existingSetting = existingSettings.FirstOrDefault(item => item.SettingName.Equals(setting.SettingName, StringComparison.OrdinalIgnoreCase));
                    if (existingSetting == null)
                    {
                        _settings.AddSetting(setting);
                        AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Create);
                        _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Created {Setting}", setting);

                    }
                    else
                    {
                        if (existingSetting.SettingValue != setting.SettingValue || (modified && existingSetting.IsPrivate != setting.IsPrivate))
                        {
                            existingSetting.SettingValue = setting.SettingValue;
                            existingSetting.IsPrivate = setting.IsPrivate;
                            _settings.UpdateSetting(existingSetting);
                            AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Update);
                            _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Updated {Setting}", setting);
                        }
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Settings For {EntityName}:{EntityId}", entityName, entityId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/site/1/settingname/settingid
        [HttpDelete("{entityName}/{entityId}/{settingName}")]
        public void Delete(string entityName, int entityId, string settingName)
        {
            Setting setting = _settings.GetSetting(entityName, entityId, settingName);
            if (setting != null && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                _settings.DeleteSetting(setting.EntityName, setting.SettingId);
                AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Setting Deleted {Setting}", setting);
            }
            else
            {
                if (entityName != EntityNames.Visitor)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Setting Does Not Exist Or User Not Authorized To Delete Setting For EntityName {EntityName} EntityId {EntityId} SettingName {SettingName}", entityName, entityId, settingName);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
        }

        // DELETE api/<controller>/1/site
        [HttpDelete("{id}/{entityName}")]
        public void Delete(int id, string entityName)
        {
            Setting setting = _settings.GetSetting(entityName, id);
            if (setting != null && IsAuthorized(setting.EntityName, setting.EntityId, PermissionNames.Edit))
            {
                _settings.DeleteSetting(setting.EntityName, setting.SettingId);
                AddSyncEvent(setting.EntityName, setting.EntityId, setting.SettingId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Setting Deleted {Setting}", setting);
            }
            else
            {
                if (entityName != EntityNames.Visitor)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Delete, "Setting Does Not Exist Or User Not Authorized To Delete Setting For SettingId {SettingId} For EntityName {EntityName} ", id, entityName);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
        }

        // GET: api/<controller>/entitynames
        [HttpGet("entitynames")]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<string> GetEntityNames()
        {
            return _settings.GetEntityNames();
        }

        // GET: api/<controller>/entityids?entityname=x
        [HttpGet("entityids")]
        [Authorize(Roles = RoleNames.Admin)]
        public IEnumerable<int> GetEntityIds(string entityName)
        {
            return _settings.GetEntityIds(entityName);
        }

        // POST api/<controller>/import?settings=x
        [HttpPost("import")]
        [Authorize(Roles = RoleNames.Host)]
        public Result Import([FromBody] Result settings)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(settings.Message))
            {
                int rows = 0;

                using (StringReader reader = new StringReader(settings.Message))
                {
                    // regex to split by comma - ignoring commas within double quotes
                    string pattern = ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)";
                    string row;

                    while ((row = reader.ReadLine()) != null)
                    {
                        List<string> cols = new List<string>();
                        string col = "";
                        int startIndex = 0;

                        MatchCollection matches = Regex.Matches(row, pattern);
                        foreach (Match match in matches)
                        {
                            col = row.Substring(startIndex, match.Index - startIndex);
                            if (col.StartsWith("\"") && col.EndsWith("\""))
                            {
                               col = col.Substring(1, col.Length - 2).Replace("\"\"", "\"");
                            }
                            cols.Add(col.Trim());
                            startIndex = match.Index + match.Length;
                        }
                        col = row.Substring(startIndex);
                        if (col.StartsWith("\"") && col.EndsWith("\""))
                        {
                            col = col.Substring(1, col.Length - 2).Replace("\"\"", "\"");
                        }
                        cols.Add(col.Trim());

                        if (cols.Count == 5 && cols[0].ToLower() != "entity" && int.TryParse(cols[1], out int entityId) && bool.TryParse(cols[4], out bool isPrivate))
                        {
                            var setting = _settings.GetSetting(cols[0], entityId, cols[2]);
                            if (setting == null)
                            {
                                _settings.AddSetting(new Setting { EntityName = cols[0], EntityId = entityId, SettingName = cols[2], SettingValue = cols[3], IsPrivate = isPrivate });
                            }
                            else
                            {
                                setting.SettingValue = cols[3];
                                setting.IsPrivate = isPrivate;
                                _settings.UpdateSetting(setting);
                            }
                            rows++;
                        }
                    }
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Settings Imported {Settings}", settings.Message);
                settings.Message = $"{rows} Settings Imported";
                settings.Success = true;
                return settings;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Settings Import Attempt {Settings}", settings.Message);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                settings.Message = "";
                settings.Success = false;
                return settings;
            }
        }

        // DELETE api/<controller>/clear
        [HttpDelete("clear")]
        [Authorize(Roles = RoleNames.Admin)]
        public void Clear()
        {
            (_cookieOptions as SiteOptionsManager<CookieAuthenticationOptions>).Reset();
            (_cookieOptionsSnapshot as SiteOptionsManager<CookieAuthenticationOptions>).Reset();
            _cookieOptionsMonitorCache.Clear();

            (_oidcOptions as SiteOptionsManager<OpenIdConnectOptions>).Reset();
            (_oidcOptionsSnapshot as SiteOptionsManager<OpenIdConnectOptions>).Reset();
            _oidcOptionsMonitorCache.Clear();

            (_oauthOptions as SiteOptionsManager<OAuthOptions>).Reset();
            (_oauthOptionsSnapshot as SiteOptionsManager<OAuthOptions>).Reset();
            _oauthOptionsMonitorCache.Clear();

            (_identityOptions as SiteOptionsManager<IdentityOptions>).Reset();
            (_identityOptionsSnapshot as SiteOptionsManager<IdentityOptions>).Reset();
            _identityOptionsMonitorCache.Clear();

            _logger.Log(LogLevel.Information, this, LogFunction.Other, "Site Options Cache Cleared");
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
                case EntityNames.Job:
                case EntityNames.Theme:
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
                case EntityNames.Role:
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
                    authorized = _userPermissions.IsAuthorized(User, _alias.SiteId, entityName, entityId, permissionName);
                    break;
                case EntityNames.User:
                    authorized = _userPermissions.IsAuthorized(User, _alias.SiteId, entityName, -1, PermissionNames.Write, RoleNames.Admin) || (_userPermissions.GetUser(User).UserId == entityId);
                    break;
                case EntityNames.Visitor:
                    authorized = User.IsInRole(RoleNames.Admin);
                    if (!authorized)
                    {
                        var visitorCookieName = Constants.VisitorCookiePrefix + _alias.SiteId.ToString();
                        authorized = (entityId == GetVisitorCookieId(HttpContext.Request.Cookies[visitorCookieName]));
                    }
                    break;
                default: // custom entity
                    authorized = true;
                    if (permissionName == PermissionNames.Edit)
                    {
                        if (entityId == -1)
                        {
                            authorized = User.IsInRole(entityName.ToLower().StartsWith("master:") ? RoleNames.Host : RoleNames.Admin);
                        }
                        else
                        {
                            authorized = _userPermissions.IsAuthorized(User, _alias.SiteId, entityName, entityId, permissionName);
                        }
                    }
                    break;
            }
            return authorized;
        }

        private bool FilterPrivate(string entityName, int entityId)
        {
            bool filter = false;
            switch (entityName)
            {
                case EntityNames.Tenant:
                case EntityNames.ModuleDefinition:
                case EntityNames.Host:
                case EntityNames.Job:
                case EntityNames.Theme:
                    filter = !User.IsInRole(RoleNames.Host);
                    break;
                case EntityNames.Site:
                case EntityNames.Role:
                    filter = !User.IsInRole(RoleNames.Admin);
                    break;
                case EntityNames.Page:
                case EntityNames.Module:
                case EntityNames.Folder:
                    filter = !_userPermissions.IsAuthorized(User, _alias.SiteId, entityName, entityId, PermissionNames.Edit);
                    break;
                case EntityNames.User:
                    filter = !_userPermissions.IsAuthorized(User, _alias.SiteId, entityName, -1, PermissionNames.Write, RoleNames.Admin) && _userPermissions.GetUser(User).UserId != entityId;
                    break;
                case EntityNames.Visitor:
                    if (!User.IsInRole(RoleNames.Admin))
                    {
                        var visitorCookieName = Constants.VisitorCookiePrefix + _alias.SiteId.ToString();
                        filter = (entityId != GetVisitorCookieId(Request.Cookies[visitorCookieName]));
                    }
                    break;
                default: // custom entity
                    filter = !User.IsInRole(entityName.ToLower().StartsWith("master:") ? RoleNames.Host : RoleNames.Admin) && !_userPermissions.IsAuthorized(User, _alias.SiteId, entityName, entityId, PermissionNames.Edit);
                    break;
            }
            return filter;
        }

        private int GetVisitorCookieId(string visitorCookie)
        {
            var visitorId = -1;
            if (visitorCookie != null)
            {
                // visitor cookies now contain the visitor id and an expiry date separated by a pipe symbol
                visitorCookie = (visitorCookie.Contains("|")) ? visitorCookie.Split('|')[0] : visitorCookie;
                visitorId = int.TryParse(visitorCookie, out int _visitorId) ? _visitorId : -1;
            }
            return visitorId;
        }

        private void AddSyncEvent(string EntityName, int EntityId, int SettingId, string Action)
        {
            _syncManager.AddSyncEvent(_alias, EntityName + "Setting", SettingId, Action);

            switch (EntityName)
            {
                case EntityNames.Module:
                case EntityNames.Page:
                case EntityNames.Site:
                    _syncManager.AddSyncEvent(_alias, EntityNames.Site, _alias.SiteId, SyncEventActions.Refresh);
                    break;
                case EntityNames.User:
                    _syncManager.AddSyncEvent(_alias, EntityName, EntityId, SyncEventActions.Update);
                    break;
            }
        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Security;
using Oqtane.Infrastructure;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SettingController : Controller
    {
        private readonly ISettingRepository _settings;
        private readonly IPageModuleRepository _pageModules;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;

        public SettingController(ISettingRepository settings, IPageModuleRepository pageModules, IUserPermissions userPermissions, ILogManager logger)
        {
            _settings = settings;
            _pageModules = pageModules;
            _userPermissions = userPermissions;
            _logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Setting> Get(string entityname, int entityid)
        {
            List<Setting> settings = new List<Setting>();
            if (IsAuthorized(entityname, entityid, "View"))
            {
                settings = _settings.GetSettings(entityname, entityid).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Settings {EntityName} {EntityId}", entityname, entityid);
                HttpContext.Response.StatusCode = 401;
            }
            return settings;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Setting Get(int id)
        {
            Setting setting = _settings.GetSetting(id);
            if (IsAuthorized(setting.EntityName, setting.EntityId, "View"))
            {
                return setting;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Setting {Setting}", setting);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        public Setting Post([FromBody] Setting Setting)
        {
            if (ModelState.IsValid && IsAuthorized(Setting.EntityName, Setting.EntityId, "Edit"))
            {
                Setting = _settings.AddSetting(Setting);
                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Setting Added {Setting}", Setting);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Setting {Setting}", Setting);
                HttpContext.Response.StatusCode = 401;
                Setting = null;
            }
            return Setting;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public Setting Put(int id, [FromBody] Setting Setting)
        {
            if (ModelState.IsValid && IsAuthorized(Setting.EntityName, Setting.EntityId, "Edit"))
            {
                Setting = _settings.UpdateSetting(Setting);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Updated {Setting}", Setting);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Setting {Setting}", Setting);
                HttpContext.Response.StatusCode = 401;
                Setting = null;
            }
            return Setting;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Setting setting = _settings.GetSetting(id);
            if (IsAuthorized(setting.EntityName, setting.EntityId, "Edit"))
            {
                _settings.DeleteSetting(id);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "Setting Deleted {Setting}", setting);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Setting {Setting}", setting);
                HttpContext.Response.StatusCode = 401;
            }
        }

        private bool IsAuthorized(string EntityName, int EntityId, string PermissionName)
        {
            bool authorized = false;
            if (EntityName == "PageModule")
            {
                EntityName = "Module";
                EntityId = _pageModules.GetPageModule(EntityId).ModuleId;
            }
            switch (EntityName)
            {
                case "Host":
                    authorized = User.IsInRole(Constants.HostRole);
                    break;
                case "Site":
                    authorized = User.IsInRole(Constants.AdminRole);
                    break;
                case "Page":
                case "Module":
                case "Folder":
                    authorized = _userPermissions.IsAuthorized(User, EntityName, EntityId, PermissionName);
                    break;
                case "User":
                    authorized = true;
                    if (PermissionName == "Edit")
                    {
                        authorized = User.IsInRole(Constants.AdminRole) || (_userPermissions.GetUser(User).UserId == EntityId);
                    }
                    break;
            }
            return authorized;
        }
    }
}

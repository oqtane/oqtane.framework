using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using Oqtane.Security;
using Oqtane.Infrastructure;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class SettingController : Controller
    {
        private readonly ISettingRepository Settings;
        private readonly IPageModuleRepository PageModules;
        private readonly IUserPermissions UserPermissions;
        private readonly IHttpContextAccessor Accessor;
        private readonly ILogManager logger;

        public SettingController(ISettingRepository Settings, IPageModuleRepository PageModules, IUserPermissions UserPermissions, IHttpContextAccessor Accessor, ILogManager logger)
        {
            this.Settings = Settings;
            this.PageModules = PageModules;
            this.UserPermissions = UserPermissions;
            this.Accessor = Accessor;
            this.logger = logger;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Setting> Get(string entityname, int entityid)
        {
            List<Setting> settings = new List<Setting>();
            if (IsAuthorized(entityname, entityid, "View"))
            {
                settings = Settings.GetSettings(entityname, entityid).ToList();
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Settings {EntityName} {EntityId}", entityname, entityid);
                HttpContext.Response.StatusCode = 401;
            }
            return settings;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Setting Get(int id)
        {
            Setting setting = Settings.GetSetting(id);
            if (IsAuthorized(setting.EntityName, setting.EntityId, "View"))
            {
                return setting;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Setting {Setting}", setting);
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
                Setting = Settings.AddSetting(Setting);
                logger.Log(LogLevel.Information, this, LogFunction.Create, "Setting Added {Setting}", Setting);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Add Setting {Setting}", Setting);
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
                Setting = Settings.UpdateSetting(Setting);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "Setting Updated {Setting}", Setting);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update Setting {Setting}", Setting);
                HttpContext.Response.StatusCode = 401;
                Setting = null;
            }
            return Setting;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            Setting setting = Settings.GetSetting(id);
            if (IsAuthorized(setting.EntityName, setting.EntityId, "Edit"))
            {
                Settings.DeleteSetting(id);
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "Setting Deleted {Setting}", setting);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete Setting {Setting}", setting);
                HttpContext.Response.StatusCode = 401;
            }
        }

        private bool IsAuthorized(string EntityName, int EntityId, string PermissionName)
        {
            bool authorized = false;
            if (EntityName == "PageModule")
            {
                EntityName = "Module";
                EntityId = PageModules.GetPageModule(EntityId).ModuleId;
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
                    authorized = UserPermissions.IsAuthorized(User, EntityName, EntityId, PermissionName);
                    break;
                case "User":
                    authorized = true;
                    if (PermissionName == "Edit")
                    {
                        authorized = User.IsInRole(Constants.AdminRole) || (int.Parse(Accessor.HttpContext.User.FindFirst(ClaimTypes.PrimarySid).Value) == EntityId);
                    }
                    break;
            }
            return authorized;
        }
    }
}

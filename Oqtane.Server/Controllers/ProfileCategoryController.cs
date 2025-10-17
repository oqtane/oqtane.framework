using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class ProfileCategoryController : Controller
    {
        private const string ProfileCategoriesSettingName = "ProfileCategories";
        private readonly IProfileRepository _profiles;
        private readonly ISettingRepository _settings;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public ProfileCategoryController(IProfileRepository profiles, ISettingRepository settings, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _profiles = profiles;
            _settings = settings;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        [HttpGet]
        [Authorize(Roles = RoleNames.Registered)]
        public IEnumerable<string> Get(int siteId)
        {
            if (siteId == _alias.SiteId)
            {
                var setting = _settings.GetSetting(EntityNames.Site, siteId, ProfileCategoriesSettingName);
                var categories = !string.IsNullOrEmpty(setting?.SettingValue) ? JsonSerializer.Deserialize<List<string>>(setting.SettingValue) : new List<string>();
                var avalableCategories = _profiles.GetProfiles(siteId).Select(i => i.Category).Distinct();
                return categories
                    .Where(i => avalableCategories.Contains(i))
                    .Concat(avalableCategories).Distinct();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Categories Get Attempt {SiteId}", siteId);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        [HttpPut]
        [Authorize(Policy = $"{EntityNames.Profile}:{PermissionNames.Write}:{RoleNames.Admin}")]
        public void Put(int siteId, [FromBody] IEnumerable<string> categories)
        {
            if (siteId == _alias.SiteId && categories != null)
            {
                var settingValue = JsonSerializer.Serialize(categories);
                var setting = _settings.GetSetting(EntityNames.Site, siteId, ProfileCategoriesSettingName);
                if (setting == null)
                {
                    setting = new Setting
                    {
                        EntityName = EntityNames.Site,
                        EntityId = siteId,
                        SettingName = ProfileCategoriesSettingName,
                        SettingValue = settingValue
                    };
                    setting = _settings.AddSetting(setting);

                }
                else
                {
                    setting.SettingValue = settingValue;
                    setting = _settings.UpdateSetting(setting);
                }
                _syncManager.AddSyncEvent(_alias, EntityNames.Site, siteId, SyncEventActions.Refresh);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "Profile Categories Updated {categories}", categories);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized Profile Categories Put Attempt {categories}", categories);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }
    }
}

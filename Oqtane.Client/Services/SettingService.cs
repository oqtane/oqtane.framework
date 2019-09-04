using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SettingService : ServiceBase, ISettingService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly IUriHelper urihelper;

        public SettingService(HttpClient http, SiteState sitestate, IUriHelper urihelper)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.urihelper = urihelper;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, urihelper.GetAbsoluteUri(), "Setting"); }
        }

        public async Task<List<Setting>> GetHostSettingsAsync()
        {
            return await GetSettingsAsync("Host", -1);
        }

        public async Task<Setting> UpdateHostSettingsAsync(List<Setting> HostSettings, string SettingName, string SettingValue)
        {
            return await UpdateSettingsAsync(HostSettings, "Host", -1, SettingName, SettingValue); 
        }

        public async Task<List<Setting>> GetSiteSettingsAsync(int SiteId)
        {
            return await GetSettingsAsync("Site", SiteId);
        }

        public async Task<Setting> UpdateSiteSettingsAsync(List<Setting> SiteSettings, int SiteId, string SettingName, string SettingValue)
        {
            return await UpdateSettingsAsync(SiteSettings, "Site", SiteId, SettingName, SettingValue);
        }

        public async Task<List<Setting>> GetPageSettingsAsync(int PageId)
        {
            return await GetSettingsAsync("Page", PageId);
        }

        public async Task<Setting> UpdatePageSettingsAsync(List<Setting> PageSettings, int PageId, string SettingName, string SettingValue)
        {
            return await UpdateSettingsAsync(PageSettings, "Page", PageId, SettingName, SettingValue);
        }

        public async Task<List<Setting>> GetPageModuleSettingsAsync(int PageModuleId)
        {
            return await GetSettingsAsync("PageModule", PageModuleId);
        }

        public async Task<Setting> UpdatePageModuleSettingsAsync(List<Setting> PageModuleSettings, int PageModuleId, string SettingName, string SettingValue)
        {
            return await UpdateSettingsAsync(PageModuleSettings, "PageModule", PageModuleId, SettingName, SettingValue);
        }

        public async Task<List<Setting>> GetModuleSettingsAsync(int ModuleId)
        {
            return await GetSettingsAsync("Module", ModuleId);
        }

        public async Task<Setting> UpdateModuleSettingsAsync(List<Setting> ModuleSettings, int ModuleId, string SettingName, string SettingValue)
        {
            return await UpdateSettingsAsync(ModuleSettings, "Module", ModuleId, SettingName, SettingValue);
        }

        public async Task<List<Setting>> GetUserSettingsAsync(int UserId)
        {
            return await GetSettingsAsync("User", UserId);
        }

        public async Task<Setting> UpdateUserSettingsAsync(List<Setting> UserSettings, int UserId, string SettingName, string SettingValue)
        {
            return await UpdateSettingsAsync(UserSettings, "User", UserId, SettingName, SettingValue);
        }

        public async Task<List<Setting>> GetSettingsAsync(string EntityName, int EntityId)
        {
            List<Setting> Settings = await http.GetJsonAsync<List<Setting>>(apiurl + "?entityname=" + EntityName + "&entityid=" + EntityId.ToString());
            return Settings.OrderBy(item => item.SettingName).ToList();
        }

        public async Task<Setting> GetSettingAsync(int SettingId)
        {
            return await http.GetJsonAsync<Setting>(apiurl + "/" + SettingId.ToString());
        }

        public async Task<Setting> AddSettingAsync(Setting Setting)
        {
            return await http.PostJsonAsync<Setting>(apiurl, Setting);
        }

        public async Task<Setting> UpdateSettingAsync(Setting Setting)
        {
            return await http.PutJsonAsync<Setting>(apiurl + "/" + Setting.SettingId.ToString(), Setting);
        }

        public async Task<Setting> UpdateSettingsAsync(List<Setting> Settings, string EntityName, int EntityId, string SettingName, string SettingValue)
        {
            Setting setting = Settings.Where(item => item.SettingName == SettingName).FirstOrDefault();
            if (setting == null || setting.SettingId == -1)
            {
                setting = new Setting();
                setting.EntityName = EntityName;
                setting.EntityId = EntityId;
                setting.SettingName = SettingName;
                setting.SettingValue = SettingValue;
                setting = await AddSettingAsync(setting);
            }
            else
            {
                setting.SettingValue = SettingValue;
                setting = await UpdateSettingAsync(setting);
            }
            return setting;
        }

        public async Task DeleteSettingAsync(int SettingId)
        {
            await http.DeleteAsync(apiurl + "/" + SettingId.ToString());
        }


        public string GetSetting(List<Setting> Settings, string SettingName, string DefaultValue)
        {
            string value = DefaultValue;
            Setting setting = Settings.Where(item => item.SettingName == SettingName).FirstOrDefault();
            if (setting != null)
            {
                value = setting.SettingValue;
            }
            return value;
        }

        public List<Setting> SetSetting(List<Setting> Settings, string EntityName, int EntityId, string SettingName, string SettingValue)
        {
            int index = Settings.FindIndex(item => item.EntityName == EntityName && item.EntityId == EntityId && item.SettingName == SettingName);
            if (index != -1)
            {
                Settings[index].SettingValue = SettingValue;
            }
            else
            {
                Settings.Add(new Setting { SettingId = -1, EntityName = EntityName, EntityId = EntityId, SettingName = SettingName, SettingValue = SettingValue });
            }
            return Settings;
        }
    }
}

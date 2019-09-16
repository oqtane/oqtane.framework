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
        private readonly NavigationManager NavigationManager;

        public SettingService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Setting"); }
        }

        public async Task<Dictionary<string, string>> GetHostSettingsAsync()
        {
            return await GetSettingsAsync("Host", -1);
        }

        public async Task UpdateHostSettingsAsync(Dictionary<string, string> HostSettings)
        {
            await UpdateSettingsAsync(HostSettings, "Host", -1); 
        }

        public async Task<Dictionary<string, string>> GetSiteSettingsAsync(int SiteId)
        {
            return await GetSettingsAsync("Site", SiteId);
        }

        public async Task UpdateSiteSettingsAsync(Dictionary<string, string> SiteSettings, int SiteId)
        {
            await UpdateSettingsAsync(SiteSettings, "Site", SiteId);
        }

        public async Task<Dictionary<string, string>> GetPageSettingsAsync(int PageId)
        {
            return await GetSettingsAsync("Page", PageId);
        }

        public async Task UpdatePageSettingsAsync(Dictionary<string, string> PageSettings, int PageId)
        {
            await UpdateSettingsAsync(PageSettings, "Page", PageId);
        }

        public async Task<Dictionary<string, string>> GetPageModuleSettingsAsync(int PageModuleId)
        {
            return await GetSettingsAsync("PageModule", PageModuleId);
        }

        public async Task UpdatePageModuleSettingsAsync(Dictionary<string, string> PageModuleSettings, int PageModuleId)
        {
            await UpdateSettingsAsync(PageModuleSettings, "PageModule", PageModuleId);
        }

        public async Task<Dictionary<string, string>> GetModuleSettingsAsync(int ModuleId)
        {
            return await GetSettingsAsync("Module", ModuleId);
        }

        public async Task UpdateModuleSettingsAsync(Dictionary<string, string> ModuleSettings, int ModuleId)
        {
            await UpdateSettingsAsync(ModuleSettings, "Module", ModuleId);
        }

        public async Task<Dictionary<string, string>> GetUserSettingsAsync(int UserId)
        {
            return await GetSettingsAsync("User", UserId);
        }

        public async Task UpdateUserSettingsAsync(Dictionary<string, string> UserSettings, int UserId)
        {
            await UpdateSettingsAsync(UserSettings, "User", UserId);
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync(string EntityName, int EntityId)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            List<Setting> Settings = await http.GetJsonAsync<List<Setting>>(apiurl + "?entityname=" + EntityName + "&entityid=" + EntityId.ToString());
            foreach(Setting setting in Settings.OrderBy(item => item.SettingName).ToList())
            {
                dictionary.Add(setting.SettingName, setting.SettingValue);
            }
            return dictionary;
        }

        public async Task UpdateSettingsAsync(Dictionary<string, string> Settings, string EntityName, int EntityId)
        {
            List<Setting> settings = await http.GetJsonAsync<List<Setting>>(apiurl + "?entityname=" + EntityName + "&entityid=" + EntityId.ToString());
            foreach (KeyValuePair<string, string> kvp in Settings)
            {
                Setting setting = settings.Where(item => item.SettingName == kvp.Key).FirstOrDefault();
                if (setting == null)
                {
                    setting = new Setting();
                    setting.EntityName = EntityName;
                    setting.EntityId = EntityId;
                    setting.SettingName = kvp.Key;
                    setting.SettingValue = kvp.Value;
                    setting = await AddSettingAsync(setting);
                }
                else
                {
                    setting.SettingValue = kvp.Value;
                    setting = await UpdateSettingAsync(setting);
                }
            }
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

        public async Task DeleteSettingAsync(int SettingId)
        {
            await http.DeleteAsync(apiurl + "/" + SettingId.ToString());
        }


        public string GetSetting(Dictionary<string, string> Settings, string SettingName, string DefaultValue)
        {
            string value = DefaultValue;
            if (Settings.ContainsKey(SettingName))
            {
                value = Settings[SettingName];
            }
            return value;
        }

        public Dictionary<string, string> SetSetting(Dictionary<string, string> Settings, string SettingName, string SettingValue)
        {
            if (Settings.ContainsKey(SettingName))
            { 
                Settings[SettingName] = SettingValue;
            }
            else
            {
                Settings.Add(SettingName, SettingValue);
            }
            return Settings;
        }
    }
}

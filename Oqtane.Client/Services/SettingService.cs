using System;
using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class SettingService : ServiceBase, ISettingService
    {
        
        private readonly SiteState _siteState;

        public SettingService(HttpClient http, SiteState siteState) : base(http)
        {            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Setting");
        public async Task<Dictionary<string, string>> GetTenantSettingsAsync()
        {
            return await GetSettingsAsync(EntityNames.Tenant, -1);
        }

        public async Task UpdateTenantSettingsAsync(Dictionary<string, string> tenantSettings)
        {
            await UpdateSettingsAsync(tenantSettings, EntityNames.Tenant, -1);
        }

        public async Task<Dictionary<string, string>> GetSiteSettingsAsync(int siteId)
        {
            return await GetSettingsAsync(EntityNames.Site, siteId);
        }

        public async Task UpdateSiteSettingsAsync(Dictionary<string, string> siteSettings, int siteId)
        {
            await UpdateSettingsAsync(siteSettings, EntityNames.Site, siteId);
        }

        public async Task<Dictionary<string, string>> GetPageSettingsAsync(int pageId)
        {
            return await GetSettingsAsync(EntityNames.Page, pageId);
        }

        public async Task UpdatePageSettingsAsync(Dictionary<string, string> pageSettings, int pageId)
        {
            await UpdateSettingsAsync(pageSettings, EntityNames.Page, pageId);
        }

        public async Task<Dictionary<string, string>> GetPageModuleSettingsAsync(int pageModuleId)
        {
            return await GetSettingsAsync(EntityNames.PageModule, pageModuleId);
        }

        public async Task UpdatePageModuleSettingsAsync(Dictionary<string, string> pageModuleSettings, int pageModuleId)
        {
            await UpdateSettingsAsync(pageModuleSettings, EntityNames.PageModule, pageModuleId);
        }

        public async Task<Dictionary<string, string>> GetModuleSettingsAsync(int moduleId)
        {
            return await GetSettingsAsync(EntityNames.Module, moduleId);
        }

        public async Task UpdateModuleSettingsAsync(Dictionary<string, string> moduleSettings, int moduleId)
        {
            await UpdateSettingsAsync(moduleSettings, EntityNames.Module, moduleId);
        }

        public async Task<Dictionary<string, string>> GetUserSettingsAsync(int userId)
        {
            return await GetSettingsAsync(EntityNames.User, userId);
        }

        public async Task UpdateUserSettingsAsync(Dictionary<string, string> userSettings, int userId)
        {
            await UpdateSettingsAsync(userSettings, EntityNames.User, userId);
        }

        public async Task<Dictionary<string, string>> GetFolderSettingsAsync(int folderId)
        {
            return await GetSettingsAsync( EntityNames.Folder, folderId);
        }

        public async Task UpdateFolderSettingsAsync(Dictionary<string, string> folderSettings, int folderId)
        {
            await UpdateSettingsAsync(folderSettings, EntityNames.Folder, folderId);
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync(string entityName, int entityId)
        {
            var dictionary = new Dictionary<string, string>();
            var settings = await GetJsonAsync<List<Setting>>($"{Apiurl}?entityname={entityName}&entityid={entityId}");
            
            foreach(Setting setting in settings.OrderBy(item => item.SettingName).ToList())
            {
                dictionary.Add(setting.SettingName, setting.SettingValue);
            }
            return dictionary;
        }

        public async Task UpdateSettingsAsync(Dictionary<string, string> settings, string entityName, int entityId)
        {
            var settingsList = await GetJsonAsync<List<Setting>>($"{Apiurl}?entityname={entityName}&entityid={entityId}");

            foreach (KeyValuePair<string, string> kvp in settings)
            {
                Setting setting = settingsList.FirstOrDefault(item => item.SettingName.Equals(kvp.Key,StringComparison.OrdinalIgnoreCase));
                if (setting == null)
                {
                    setting = new Setting();
                    setting.EntityName = entityName;
                    setting.EntityId = entityId;
                    setting.SettingName = kvp.Key;
                    setting.SettingValue = kvp.Value;
                    setting = await AddSettingAsync(setting);
                }
                else
                {
                    if (setting.SettingValue != kvp.Value)
                    {
                        setting.SettingValue = kvp.Value;
                        setting = await UpdateSettingAsync(setting);
                    }
                }
            }
        }


        public async Task<Setting> GetSettingAsync(int settingId)
        {
            return await GetJsonAsync<Setting>($"{Apiurl}/{settingId}");
        }

        public async Task<Setting> AddSettingAsync(Setting setting)
        {
            return await PostJsonAsync<Setting>(Apiurl, setting);
        }

        public async Task<Setting> UpdateSettingAsync(Setting setting)
        {
            return await PutJsonAsync<Setting>($"{Apiurl}/{setting.SettingId}", setting);
        }

        public async Task DeleteSettingAsync(int settingId)
        {
            await DeleteAsync($"{Apiurl}/{settingId}");
        }


        public string GetSetting(Dictionary<string, string> settings, string settingName, string defaultValue)
        {
            string value = defaultValue;
            if (settings != null && settings.ContainsKey(settingName))
            {
                value = settings[settingName];
            }
            return value;
        }

        public Dictionary<string, string> SetSetting(Dictionary<string, string> settings, string settingName, string settingValue)
        {
            if (settings == null)
            {
                settings = new Dictionary<string, string>();
            }
            if (settings.ContainsKey(settingName))
            { 
                settings[settingName] = settingValue;
            }
            else
            {
                settings.Add(settingName, settingValue);
            }
            return settings;
        }
    }
}

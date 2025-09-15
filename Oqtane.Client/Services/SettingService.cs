using System;
using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Setting"/>s
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// Returns a key-value dictionary of all tenant settings
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetTenantSettingsAsync();

        /// <summary>
        /// Updates a tenant setting
        /// </summary>
        /// <param name="tenantSettings"></param>
        /// <returns></returns>
        Task UpdateTenantSettingsAsync(Dictionary<string, string> tenantSettings);

        /// <summary>
        ///  Returns a key-value dictionary of all site settings for the given site
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetSiteSettingsAsync(int siteId);

        /// <summary>
        /// Updates a site setting
        /// </summary>
        /// <param name="siteSettings"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task UpdateSiteSettingsAsync(Dictionary<string, string> siteSettings, int siteId);

        /// <summary>
        /// Clears site option cache
        /// </summary>
        /// <returns></returns>
        Task ClearSiteSettingsCacheAsync();

        /// <summary>
        /// Returns a key-value dictionary of all page settings for the given page
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPageSettingsAsync(int pageId);

        /// <summary>
        ///  Updates a page setting
        /// </summary>
        /// <param name="pageSettings"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task UpdatePageSettingsAsync(Dictionary<string, string> pageSettings, int pageId);

        /// <summary>
        /// Returns a key-value dictionary of all page module settings for the given page module
        /// </summary>
        /// <param name="pageModuleId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetPageModuleSettingsAsync(int pageModuleId);

        /// <summary>
        /// Updates a page module setting
        /// </summary>
        /// <param name="pageModuleSettings"></param>
        /// <param name="pageModuleId"></param>
        /// <returns></returns>
        Task UpdatePageModuleSettingsAsync(Dictionary<string, string> pageModuleSettings, int pageModuleId);

        /// <summary>
        /// Returns a key-value dictionary of all module settings for the given module
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetModuleSettingsAsync(int moduleId);

        /// <summary>
        /// Updates a module setting
        /// </summary>
        /// <param name="moduleSettings"></param>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        Task UpdateModuleSettingsAsync(Dictionary<string, string> moduleSettings, int moduleId);

        /// <summary>
        /// Returns a key-value dictionary of all module settings for the given module
        /// </summary>
        /// <param name="moduleDefinitionId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetModuleDefinitionSettingsAsync(int moduleDefinitionId);

        /// <summary>
        /// Updates a module setting
        /// </summary>
        /// <param name="moduleDefinitionSettings"></param>
        /// <param name="moduleDefinitionId"></param>
        /// <returns></returns>
        Task UpdateModuleDefinitionSettingsAsync(Dictionary<string, string> moduleDefinitionSettings, int moduleDefinitionId);

        /// <summary>
        /// Returns a key-value dictionary of all user settings for the given user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetUserSettingsAsync(int userId);

        /// <summary>
        /// Updates a user setting
        /// </summary>
        /// <param name="userSettings"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task UpdateUserSettingsAsync(Dictionary<string, string> userSettings, int userId);

        /// <summary>
        /// Returns a key-value dictionary of all folder settings for the given folder
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetFolderSettingsAsync(int folderId);


        /// <summary>
        /// Updates a folder setting
        /// </summary>
        /// <param name="folderSettings"></param>
        /// <param name="folderId"></param>
        /// <returns></returns>        
        Task UpdateFolderSettingsAsync(Dictionary<string, string> folderSettings, int folderId);

        /// <summary>
        /// Returns a key-value dictionary of all tenant settings
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetHostSettingsAsync();

        /// <summary>
        /// Updates a host setting
        /// </summary>
        /// <param name="hostSettings"></param>
        /// <returns></returns>
        Task UpdateHostSettingsAsync(Dictionary<string, string> hostSettings);

        /// <summary>
        /// Returns a key-value dictionary of all settings for the given visitor
        /// </summary>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetVisitorSettingsAsync(int visitorId);

        /// <summary>
        /// Updates a visitor setting
        /// </summary>
        /// <param name="visitorSettings"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        Task UpdateVisitorSettingsAsync(Dictionary<string, string> visitorSettings, int visitorId);

        /// <summary>
        /// Returns a key-value dictionary of all settings for the given entityName
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        Task<Dictionary<string, string>> GetSettingsAsync(string entityName, int entityId);

        /// <summary>
        /// Updates settings for a given entityName and Id
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Task UpdateSettingsAsync(Dictionary<string, string> settings, string entityName, int entityId);

        /// <summary>
        /// Updates setting for a given entityName and Id
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="settingName"></param>
        /// <param name="settingValue"></param>
        /// <param name="isPrivate"></param>
        /// <returns></returns>
        Task AddOrUpdateSettingAsync(string entityName, int entityId, string settingName, string settingValue, bool isPrivate);

        /// <summary>
        /// Returns a specific setting
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        Task DeleteSettingAsync(string entityName, int entityId, string settingName);

        /// <summary>
        /// Returns a specific setting
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="entityId"></param>
        /// <param name="settingName"></param>
        /// <returns></returns>
        Task<List<Setting>> GetSettingsAsync(string entityName, int entityId, string settingName);

        /// <summary>
        /// Returns a specific setting
        /// </summary>
        /// <param name="settingId"></param>
        /// <returns></returns>
        Task<Setting> GetSettingAsync(string entityName, int settingId);

        /// <summary>
        /// Creates a new setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        Task<Setting> AddSettingAsync(Setting setting);

        /// <summary>
        /// Updates a existing setting
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        Task<Setting> UpdateSettingAsync(Setting setting);

        /// <summary>
        /// Deletes a setting
        /// </summary>
        /// <param name="settingId"></param>
        /// <returns></returns>
        Task DeleteSettingAsync(string entityName, int settingId);

        /// <summary>
        /// Gets list of unique entity names
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetEntityNamesAsync();

        /// <summary>
        /// Gets a list of unique entity IDs for the given entity name
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        Task<List<int>> GetEntityIdsAsync(string entityName);

        /// <summary>
        /// Imports a list of settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<Result> ImportSettingsAsync(Result settings);

        /// <summary>
        /// Gets the value of the given settingName (key) from the given key-value dictionary 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string GetSetting(Dictionary<string, string> settings, string settingName, string defaultValue);

        /// <summary>
        /// Sets the value of the given settingName (key) in the given key-value dictionary 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="settingName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        Dictionary<string, string> SetSetting(Dictionary<string, string> settings, string settingName, string settingValue);

        Dictionary<string, string> SetSetting(Dictionary<string, string> settings, string settingName, string settingValue, bool isPrivate);

        Dictionary<string, string> MergeSettings(Dictionary<string, string> baseSettings, Dictionary<string, string> overwriteSettings);


        [Obsolete("GetSettingAsync(int settingId) is deprecated. Use GetSettingAsync(string entityName, int settingId) instead.", false)]
        Task<Setting> GetSettingAsync(int settingId);

        [Obsolete("DeleteSettingAsync(int settingId) is deprecated. Use DeleteSettingAsync(string entityName, int settingId) instead.", false)]
        Task DeleteSettingAsync(int settingId);

    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SettingService : ServiceBase, ISettingService
    {
        public SettingService(HttpClient http, SiteState siteState) : base(http, siteState) {}

        private string Apiurl => CreateApiUrl("Setting");

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

        public async Task ClearSiteSettingsCacheAsync()
        {
            await DeleteAsync($"{Apiurl}/clear");
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

        public async Task<Dictionary<string, string>> GetModuleDefinitionSettingsAsync(int moduleDefinitionId)
        {
            return await GetSettingsAsync(EntityNames.ModuleDefinition, moduleDefinitionId);
        }

        public async Task UpdateModuleDefinitionSettingsAsync(Dictionary<string, string> moduleDefinitionSettings, int moduleDefinitionId)
        {
            await UpdateSettingsAsync(moduleDefinitionSettings, EntityNames.ModuleDefinition, moduleDefinitionId);
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

        public async Task<Dictionary<string, string>> GetHostSettingsAsync()
        {
            return await GetSettingsAsync(EntityNames.Host, -1);
        }

        public async Task UpdateHostSettingsAsync(Dictionary<string, string> hostSettings)
        {
            await UpdateSettingsAsync(hostSettings, EntityNames.Host, -1);
        }

        public async Task<Dictionary<string, string>> GetVisitorSettingsAsync(int visitorId)
        {
            if (visitorId != -1)
            {
                return await GetSettingsAsync(EntityNames.Visitor, visitorId);
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        public async Task UpdateVisitorSettingsAsync(Dictionary<string, string> visitorSettings, int visitorId)
        {
            if (visitorId != -1)
            {
                await UpdateSettingsAsync(visitorSettings, EntityNames.Visitor, visitorId);
            }
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync(string entityName, int entityId)
        {
            var dictionary = new Dictionary<string, string>();
            var settings = await GetSettingsAsync(entityName, entityId, "");
            if (settings != null)
            {
                foreach (Setting setting in settings.OrderBy(item => item.SettingName).ToList())
                {
                    dictionary.Add(setting.SettingName, setting.SettingValue);
                }
            }
            return dictionary;
        }

        public async Task UpdateSettingsAsync(Dictionary<string, string> settings, string entityName, int entityId)
        {
            var settingsList = new List<Setting>();

            foreach (KeyValuePair<string, string> kvp in settings)
            {
                var setting = new Setting();
                setting.EntityName = entityName;
                setting.EntityId = entityId;
                setting.SettingName = kvp.Key;
                setting.SettingValue = kvp.Value;
                setting.IsPrivate = true;
                settingsList.Add(setting);
            }

            await PutJsonAsync<List<Setting>>($"{Apiurl}/{entityName}/{entityId}", settingsList);
        }

        public async Task AddOrUpdateSettingAsync(string entityName, int entityId, string settingName, string settingValue, bool isPrivate)
        {
            await PutAsync($"{Apiurl}/{entityName}/{entityId}/{settingName}/{settingValue}/{isPrivate}");
        }

        public async Task DeleteSettingAsync(string entityName, int entityId, string settingName)
        {
            await DeleteAsync($"{Apiurl}/{entityName}/{entityId}/{settingName}");
        }

        public async Task<List<Setting>> GetSettingsAsync(string entityName, int entityId, string settingName)
        {
            var settings = await GetJsonAsync<List<Setting>>($"{Apiurl}?entityname={entityName}&entityid={entityId}");
            if (!string.IsNullOrEmpty(settingName))
            {
                settings = settings.Where(item => item.SettingName == settingName).ToList();
            }
            return settings;
        }

        public async Task<Setting> GetSettingAsync(string entityName, int settingId)
        {
            return await GetJsonAsync<Setting>($"{Apiurl}/{settingId}/{entityName}");
        }

        public async Task<Setting> AddSettingAsync(Setting setting)
        {
            return await PostJsonAsync<Setting>(Apiurl, setting);
        }

        public async Task<Setting> UpdateSettingAsync(Setting setting)
        {
            return await PutJsonAsync<Setting>($"{Apiurl}/{setting.SettingId}", setting);
        }

        public async Task DeleteSettingAsync(string entityName, int settingId)
        {
            await DeleteAsync($"{Apiurl}/{settingId}/{entityName}");
        }

        public async Task<List<string>> GetEntityNamesAsync()
        {
            return await GetJsonAsync<List<string>>($"{Apiurl}/entitynames");
        }

        public async Task<List<int>> GetEntityIdsAsync(string entityName)
        {
            return await GetJsonAsync<List<int>>($"{Apiurl}/entityids?entityname={entityName}");
        }

        public async Task<Result> ImportSettingsAsync(Result settings)
        {
            return await PostJsonAsync<Result>($"{Apiurl}/import", settings);
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
            return SetSetting(settings, settingName, settingValue, false);
        }

        public Dictionary<string, string> SetSetting(Dictionary<string, string> settings, string settingName, string settingValue, bool isPrivate)
        {
            if (settings == null)
            {
                settings = new Dictionary<string, string>();
            }
            settingValue = (isPrivate) ? "[Private]" + settingValue : "[Public]" + settingValue;
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

        public Dictionary<string, string> MergeSettings(Dictionary<string, string> baseSettings, Dictionary<string, string> overwriteSettings)
        {
            var settings = baseSettings != null ? new Dictionary<string, string>(baseSettings) : new Dictionary<string, string>();
            if (overwriteSettings != null)
            {
                foreach (var setting in overwriteSettings)
                {
                    if (settings.ContainsKey(setting.Key))
                    {
                        settings[setting.Key] = setting.Value;
                    }
                    else
                    {
                        settings.Add(setting.Key, setting.Value);
                    }
                }
            }

            return settings;
        }

        [Obsolete("GetSettingAsync(int settingId) is deprecated. Use GetSettingAsync(string entityName, int settingId) instead.", false)]
        public async Task<Setting> GetSettingAsync(int settingId)
        {
            return await GetJsonAsync<Setting>($"{Apiurl}/{settingId}/tenant");
        }

        [Obsolete("DeleteSettingAsync(int settingId) is deprecated. Use DeleteSettingAsync(string entityName, int settingId) instead.", false)]
        public async Task DeleteSettingAsync(int settingId)
        {
            await DeleteAsync($"{Apiurl}/{settingId}/tenant");
        }


    }
}

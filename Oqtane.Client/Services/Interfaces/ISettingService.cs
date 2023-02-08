using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        Dictionary<string, string> MergeSettings(Dictionary<string, string> settings1, Dictionary<string, string> settings2);


        [Obsolete("GetSettingAsync(int settingId) is deprecated. Use GetSettingAsync(string entityName, int settingId) instead.", false)]
        Task<Setting> GetSettingAsync(int settingId);

        [Obsolete("DeleteSettingAsync(int settingId) is deprecated. Use DeleteSettingAsync(string entityName, int settingId) instead.", false)]
        Task DeleteSettingAsync(int settingId);

    }
}

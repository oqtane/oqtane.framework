using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISettingService
    {
        Task<Dictionary<string, string>> GetHostSettingsAsync();

        Task UpdateHostSettingsAsync(Dictionary<string, string> hostSettings);

        Task<Dictionary<string, string>> GetSiteSettingsAsync(int siteId);

        Task UpdateSiteSettingsAsync(Dictionary<string, string> siteSettings, int siteId);

        Task<Dictionary<string, string>> GetPageSettingsAsync(int pageId);

        Task UpdatePageSettingsAsync(Dictionary<string, string> pageSettings, int pageId);

        Task<Dictionary<string, string>> GetPageModuleSettingsAsync(int pageModuleId);

        Task UpdatePageModuleSettingsAsync(Dictionary<string, string> pageModuleSettings, int pageModuleId);

        Task<Dictionary<string, string>> GetModuleSettingsAsync(int moduleId);

        Task UpdateModuleSettingsAsync(Dictionary<string, string> moduleSettings, int moduleId);

        Task<Dictionary<string, string>> GetUserSettingsAsync(int userId);

        Task UpdateUserSettingsAsync(Dictionary<string, string> userSettings, int userId);

        Task<Dictionary<string, string>> GetFolderSettingsAsync(int folderId);

        Task UpdateFolderSettingsAsync(Dictionary<string, string> folderSettings, int folderId);

        Task<Dictionary<string, string>> GetSettingsAsync(string entityName, int entityId);

        Task UpdateSettingsAsync(Dictionary<string, string> settings, string entityName, int entityId);


        Task<Setting> GetSettingAsync(int settingId);

        Task<Setting> AddSettingAsync(Setting setting);

        Task<Setting> UpdateSettingAsync(Setting setting);

        Task DeleteSettingAsync(int settingId);


        string GetSetting(Dictionary<string, string> settings, string settingName, string defaultValue);

        Dictionary<string, string> SetSetting(Dictionary<string, string> settings, string settingName, string settingValue);
     }
}

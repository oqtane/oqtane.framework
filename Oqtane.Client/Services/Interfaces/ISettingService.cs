using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISettingService
    {
        Task<Dictionary<string, string>> GetHostSettingsAsync();

        Task UpdateHostSettingsAsync(Dictionary<string, string> HostSettings);

        Task<Dictionary<string, string>> GetSiteSettingsAsync(int SiteId);

        Task UpdateSiteSettingsAsync(Dictionary<string, string> SiteSettings, int SiteId);

        Task<Dictionary<string, string>> GetPageSettingsAsync(int PageId);

        Task UpdatePageSettingsAsync(Dictionary<string, string> PageSettings, int PageId);

        Task<Dictionary<string, string>> GetPageModuleSettingsAsync(int PageModuleId);

        Task UpdatePageModuleSettingsAsync(Dictionary<string, string> PageModuleSettings, int PageModuleId);

        Task<Dictionary<string, string>> GetModuleSettingsAsync(int ModuleId);

        Task UpdateModuleSettingsAsync(Dictionary<string, string> ModuleSettings, int ModuleId);

        Task<Dictionary<string, string>> GetUserSettingsAsync(int UserId);

        Task UpdateUserSettingsAsync(Dictionary<string, string> UserSettings, int UserId);

        Task<Dictionary<string, string>> GetSettingsAsync(string EntityName, int EntityId);

        Task UpdateSettingsAsync(Dictionary<string, string> Settings, string EntityName, int EntityId);


        Task<Setting> GetSettingAsync(int SettingId);

        Task<Setting> AddSettingAsync(Setting Setting);

        Task<Setting> UpdateSettingAsync(Setting Setting);

        Task DeleteSettingAsync(int SettingId);


        string GetSetting(Dictionary<string, string> Settings, string SettingName, string DefaultValue);

        Dictionary<string, string> SetSetting(Dictionary<string, string> Settings, string SettingName, string SettingValue);
     }
}

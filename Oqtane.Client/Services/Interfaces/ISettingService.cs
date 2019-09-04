using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISettingService
    {
        Task<List<Setting>> GetHostSettingsAsync();

        Task<Setting> UpdateHostSettingsAsync(List<Setting> HostSettings, string SettingName, string SettingValue);

        Task<List<Setting>> GetSiteSettingsAsync(int SiteId);

        Task<Setting> UpdateSiteSettingsAsync(List<Setting> SiteSettings, int SiteId, string SettingName, string SettingValue);

        Task<List<Setting>> GetPageSettingsAsync(int PageId);

        Task<Setting> UpdatePageSettingsAsync(List<Setting> PageSettings, int PageId, string SettingName, string SettingValue);

        Task<List<Setting>> GetPageModuleSettingsAsync(int PageModuleId);

        Task<Setting> UpdatePageModuleSettingsAsync(List<Setting> PageModuleSettings, int PageModuleId, string SettingName, string SettingValue);

        Task<List<Setting>> GetModuleSettingsAsync(int ModuleId);

        Task<Setting> UpdateModuleSettingsAsync(List<Setting> ModuleSettings, int ModuleId, string SettingName, string SettingValue);

        Task<List<Setting>> GetUserSettingsAsync(int UserId);

        Task<Setting> UpdateUserSettingsAsync(List<Setting> UserSettings, int UserId, string SettingName, string SettingValue);


        Task<List<Setting>> GetSettingsAsync(string EntityName, int EntityId);

        Task<Setting> GetSettingAsync(int SettingId);

        Task<Setting> AddSettingAsync(Setting Setting);

        Task<Setting> UpdateSettingAsync(Setting Setting);

        Task<Setting> UpdateSettingsAsync(List<Setting> Settings, string EntityName, int EntityId, string SettingName, string SettingValue);

        Task DeleteSettingAsync(int SettingId);


        string GetSetting(List<Setting> Settings, string SettingName, string DefaultValue);

        List<Setting> SetSetting(List<Setting> Settings, string EntityName, int EntityId, string SettingName, string SettingValue);
     }
}

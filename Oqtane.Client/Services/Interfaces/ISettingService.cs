using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ISettingService
    {
        Task<List<Setting>> GetModuleSettingsAsync(int ModuleId);

        Task<Setting> UpdateModuleSettingsAsync(List<Setting> ModuleSettings, int ModuleId, string SettingName, string SettingValue);


        Task<List<Setting>> GetSettingsAsync(string EntityName, int EntityId);

        Task<Setting> GetSettingAsync(int SettingId);

        Task<Setting> AddSettingAsync(Setting Setting);

        Task<Setting> UpdateSettingAsync(Setting Setting);

        Task DeleteSettingAsync(int SettingId);


        string GetSetting(List<Setting> Settings, string SettingName, string DefaultValue);
    }
}

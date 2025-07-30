using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface ISettingRepository
    {
        IEnumerable<Setting> GetSettings(string entityName);
        IEnumerable<Setting> GetSettings(string entityName, int entityId);
        IEnumerable<Setting> GetSettings(string entityName1, int entityId1, string entityName2, int entityId2);
        Setting AddSetting(Setting setting);
        Setting UpdateSetting(Setting setting);
        Setting GetSetting(string entityName, int settingId);
        Setting GetSetting(string entityName, int entityId, string settingName);
        void DeleteSetting(string entityName, int settingId);
        void DeleteSettings(string entityName, int entityId);
        string GetSettingValue(IEnumerable<Setting> settings, string settingName, string defaultValue);
        string GetSettingValue(string entityName, int entityId, string settingName, string defaultValue);
    }
}

using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISettingRepository
    {
        IEnumerable<Setting> GetSettings(string entityName);
        IEnumerable<Setting> GetSettings(string entityName, int entityId);
        Setting AddSetting(Setting setting);
        Setting UpdateSetting(Setting setting);
        Setting GetSetting(string entityName, int settingId);
        Setting GetSetting(string entityName, int entityId, string settingName);
        void DeleteSetting(string entityName, int settingId);
        void DeleteSettings(string entityName, int entityId);
    }
}

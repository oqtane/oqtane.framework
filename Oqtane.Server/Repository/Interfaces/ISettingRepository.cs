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
        Setting GetSetting(int settingId);
        void DeleteSetting(int settingId);
    }
}

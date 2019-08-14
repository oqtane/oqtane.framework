using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISettingRepository
    {
        IEnumerable<Setting> GetSettings(string EntityName, int EntityId);
        Setting AddSetting(Setting Setting);
        Setting UpdateSetting(Setting Setting);
        Setting GetSetting(int SettingId);
        void DeleteSetting(int SettingId);
    }
}

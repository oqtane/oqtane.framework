using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SettingRepository : ISettingRepository
    {
        private TenantDBContext _db;

        public SettingRepository(TenantDBContext context)
        {
            _db = context;
        }

        public IEnumerable<Setting> GetSettings(string entityName, int entityId)
        {
            return _db.Setting.Where(item => item.EntityName == entityName)
                .Where(item => item.EntityId == entityId);
        }

        public Setting AddSetting(Setting setting)
        {
            _db.Setting.Add(setting);
            _db.SaveChanges();
            return setting;
        }

        public Setting UpdateSetting(Setting setting)
        {
            _db.Entry(setting).State = EntityState.Modified;
            _db.SaveChanges();
            return setting;
        }

        public Setting GetSetting(int settingId)
        {
            return _db.Setting.Find(settingId);
        }

        public void DeleteSetting(int settingId)
        {
            Setting setting = _db.Setting.Find(settingId);
            _db.Setting.Remove(setting);
            _db.SaveChanges();
        }
    }
}

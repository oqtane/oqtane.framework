using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        public IEnumerable<Setting> GetSettings(string EntityName, int EntityId)
        {
            return _db.Setting.Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId);
        }

        public Setting AddSetting(Setting Setting)
        {
            _db.Setting.Add(Setting);
            _db.SaveChanges();
            return Setting;
        }

        public Setting UpdateSetting(Setting Setting)
        {
            _db.Entry(Setting).State = EntityState.Modified;
            _db.SaveChanges();
            return Setting;
        }

        public Setting GetSetting(int SettingId)
        {
            return _db.Setting.Find(SettingId);
        }

        public void DeleteSetting(int SettingId)
        {
            Setting Setting = _db.Setting.Find(SettingId);
            _db.Setting.Remove(Setting);
            _db.SaveChanges();
        }
    }
}

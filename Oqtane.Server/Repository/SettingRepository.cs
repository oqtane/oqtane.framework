using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SettingRepository : ISettingRepository
    {
        private TenantDBContext db;

        public SettingRepository(TenantDBContext context)
        {
            db = context;
        }

        public IEnumerable<Setting> GetSettings(string EntityName, int EntityId)
        {
            return db.Setting.Where(item => item.EntityName == EntityName)
                .Where(item => item.EntityId == EntityId);
        }

        public Setting AddSetting(Setting Setting)
        {
            db.Setting.Add(Setting);
            db.SaveChanges();
            return Setting;
        }

        public Setting UpdateSetting(Setting Setting)
        {
            db.Entry(Setting).State = EntityState.Modified;
            db.SaveChanges();
            return Setting;
        }

        public Setting GetSetting(int SettingId)
        {
            return db.Setting.Find(SettingId);
        }

        public void DeleteSetting(int SettingId)
        {
            Setting Setting = db.Setting.Find(SettingId);
            db.Setting.Remove(Setting);
            db.SaveChanges();
        }
    }
}

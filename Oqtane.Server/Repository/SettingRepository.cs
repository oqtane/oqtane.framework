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
            try
            {
                return db.Setting.Where(item => item.EntityName == EntityName)
                    .Where(item => item.EntityId == EntityId).ToList();
            }
            catch
            {
                throw;
            }
        }

        public Setting AddSetting(Setting Setting)
        {
            try
            {
                db.Setting.Add(Setting);
                db.SaveChanges();
                return Setting;
            }
            catch
            {
                throw;
            }
        }

        public Setting UpdateSetting(Setting Setting)
        {
            try
            {
                db.Entry(Setting).State = EntityState.Modified;
                db.SaveChanges();
                return Setting;
            }
            catch
            {
                throw;
            }
        }

        public Setting GetSetting(int SettingId)
        {
            try
            {
                return db.Setting.Find(SettingId);
            }
            catch
            {
                throw;
            }
        }

        public void DeleteSetting(int SettingId)
        {
            try
            {
                Setting Setting = db.Setting.Find(SettingId);
                db.Setting.Remove(Setting);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}

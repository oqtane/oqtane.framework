using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class SettingRepository : ISettingRepository
    {
        private TenantDBContext _tenant;
        private MasterDBContext _master;
        private readonly ITenantManager _tenantManager;
        private readonly IMemoryCache _cache;

        public SettingRepository(TenantDBContext tenant, MasterDBContext master, ITenantManager tenantManager, IMemoryCache cache)
        {
            _tenant = tenant;
            _master = master;
            _tenantManager = tenantManager;
            _cache = cache;
        }

        public IEnumerable<Setting> GetSettings(string entityName)
        {
            if (IsMaster(entityName))
            {
                return _master.Setting.Where(item => item.EntityName == entityName);
            }
            else
            {
                return _tenant.Setting.Where(item => item.EntityName == entityName);
            }
        }

        public IEnumerable<Setting> GetSettings(string entityName, int entityId)
        {
            var settings = GetSettings(entityName);
            return settings.Where(item => item.EntityId == entityId);
        }

        public Setting AddSetting(Setting setting)
        {
            if (IsMaster(setting.EntityName))
            {
                _master.Setting.Add(setting);
                _master.SaveChanges();
            }
            else
            {
                _tenant.Setting.Add(setting);
                _tenant.SaveChanges();
            }
            ManageCache(setting.EntityName);
            return setting;
        }

        public Setting UpdateSetting(Setting setting)
        {
            if (IsMaster(setting.EntityName))
            {
                _master.Entry(setting).State = EntityState.Modified;
                _master.SaveChanges();
            }
            else
            {
                _tenant.Entry(setting).State = EntityState.Modified;
                _tenant.SaveChanges();
            }
            ManageCache(setting.EntityName);
            return setting;
        }

        public Setting GetSetting(string entityName, int settingId)
        {
            if (IsMaster(entityName))
            {
                return _master.Setting.Find(settingId);
            }
            else
            {
                return _tenant.Setting.Find(settingId);
            }
        }

        public Setting GetSetting(string entityName, int entityId, string settingName)
        {
            if (IsMaster(entityName))
            {
                return _master.Setting.Where(item => item.EntityName == entityName && item.EntityId == entityId && item.SettingName == settingName).FirstOrDefault();
            }
            else
            {
                return _tenant.Setting.Where(item => item.EntityName == entityName && item.EntityId == entityId && item.SettingName == settingName).FirstOrDefault();
            }
        }

        public void DeleteSetting(string entityName, int settingId)
        {
            if (IsMaster(entityName))
            {
                Setting setting = _master.Setting.Find(settingId);
                _master.Setting.Remove(setting);
                _master.SaveChanges();
            }
            else
            {
                Setting setting = _tenant.Setting.Find(settingId);
                _tenant.Setting.Remove(setting);
                _tenant.SaveChanges();
            }
            ManageCache(entityName);
        }

        public void DeleteSettings(string entityName, int entityId)
        {
            if (IsMaster(entityName))
            {
                IEnumerable<Setting> settings = _master.Setting
                    .Where(item => item.EntityName == entityName)
                    .Where(item => item.EntityId == entityId);
                foreach (Setting setting in settings)
                {
                    _master.Setting.Remove(setting);
                }
                _master.SaveChanges();
            }
            else
            {
                IEnumerable<Setting> settings = _tenant.Setting
                    .Where(item => item.EntityName == entityName)
                    .Where(item => item.EntityId == entityId);
                foreach (Setting setting in settings)
                {
                    _tenant.Setting.Remove(setting);
                }
                _tenant.SaveChanges();
            }
            ManageCache(entityName);
        }

        private bool IsMaster(string EntityName)
        {
            return (EntityName == EntityNames.ModuleDefinition || EntityName == EntityNames.Host);
        }

        private void ManageCache(string EntityName)
        {
            if (EntityName == EntityNames.Site)
            {
                _cache.Remove(Constants.HttpContextSiteSettingsKey + _tenantManager.GetAlias().SiteKey);
            }
        }
    }
}

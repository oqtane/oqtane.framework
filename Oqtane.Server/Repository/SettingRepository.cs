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
        private readonly IDbContextFactory<TenantDBContext> _tenantContextFactory;
        private MasterDBContext _master;
        private readonly ITenantManager _tenantManager;
        private readonly IMemoryCache _cache;

        public SettingRepository(IDbContextFactory<TenantDBContext> tenantContextFactory, MasterDBContext master, ITenantManager tenantManager, IMemoryCache cache)
        {
            _tenantContextFactory = tenantContextFactory;
            _master = master;
            _tenantManager = tenantManager;
            _cache = cache;
        }

        public IEnumerable<Setting> GetSettings(string entityName)
        {
            if (IsMaster(entityName))
            {
                return _master.Setting.Where(item => item.EntityName == entityName).ToList();
            }
            else
            {
                using var db = _tenantContextFactory.CreateDbContext();
                return db.Setting.Where(item => item.EntityName == entityName).ToList();
            }
        }

        public IEnumerable<Setting> GetSettings(string entityName, int entityId)
        {
            return GetSettings(entityName).Where(item => item.EntityId == entityId);
        }

        public IEnumerable<Setting> GetSettings(string entityName1, int entityId1, string entityName2, int entityId2)
        {
            // merge settings from entity2 into entity1
            var settings1 = GetSettings(entityName1, entityId1).ToList();
            foreach (var setting2 in GetSettings(entityName2, entityId2))
            {
                var setting1 = settings1.FirstOrDefault(item => item.SettingName == setting2.SettingName);
                if (setting1 == null)
                {
                    settings1.Add(new Setting { EntityName = entityName1, EntityId = entityId1, SettingName = setting2.SettingName, SettingValue = setting2.SettingValue, IsPrivate = setting2.IsPrivate });
                }
                else
                {
                    setting1.SettingValue = setting2.SettingValue;
                    setting1.IsPrivate = setting2.IsPrivate;
                }
            }
            return settings1;
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
                using var tenant = _tenantContextFactory.CreateDbContext();
                tenant.Setting.Add(setting);
                tenant.SaveChanges();
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
                using var tenant = _tenantContextFactory.CreateDbContext();
                tenant.Entry(setting).State = EntityState.Modified;
                tenant.SaveChanges();
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
                using var tenant = _tenantContextFactory.CreateDbContext();
                return tenant.Setting.Find(settingId);
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
                using var tenant = _tenantContextFactory.CreateDbContext();
                return tenant.Setting.Where(item => item.EntityName == entityName && item.EntityId == entityId && item.SettingName == settingName).FirstOrDefault();
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
                using var tenant = _tenantContextFactory.CreateDbContext();
                Setting setting = tenant.Setting.Find(settingId);
                tenant.Setting.Remove(setting);
                tenant.SaveChanges();
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
                using var tenant = _tenantContextFactory.CreateDbContext();
                IEnumerable<Setting> settings = tenant.Setting
                    .Where(item => item.EntityName == entityName)
                    .Where(item => item.EntityId == entityId);
                foreach (Setting setting in settings)
                {
                    tenant.Setting.Remove(setting);
                }
                tenant.SaveChanges();
            }
            ManageCache(entityName);
        }

        public string GetSettingValue(IEnumerable<Setting> settings, string settingName, string defaultValue)
        {
            var setting = settings.FirstOrDefault(item => item.SettingName == settingName);
            if (setting != null)
            {
                return setting.SettingValue;
            }
            else
            {
                return defaultValue;
            }
        }

        public string GetSettingValue(string entityName, int entityId, string settingName, string defaultValue)
        {
            var setting = GetSetting(entityName, entityId, settingName);
            if (setting != null)
            {
                return setting.SettingValue;
            }
            else
            {
                return defaultValue;
            }
        }

        private bool IsMaster(string EntityName)
        {
            return (EntityName == EntityNames.ModuleDefinition || EntityName == EntityNames.Host);
        }

        private void ManageCache(string EntityName)
        {
            if (EntityName == EntityNames.Site && _tenantManager.GetAlias() != null)
            {
                _cache.Remove(Constants.HttpContextSiteSettingsKey + _tenantManager.GetAlias().SiteKey);
            }
        }
    }
}

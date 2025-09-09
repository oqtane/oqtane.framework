using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Modules.Admin.Users;
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
        IEnumerable<string> GetEntityNames();
        IEnumerable<int> GetEntityIds(string entityName);

        string GetSettingValue(IEnumerable<Setting> settings, string settingName, string defaultValue);
        string GetSettingValue(string entityName, int entityId, string settingName, string defaultValue);
    }

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
            if (IsMaster(entityName))
            {
                return _master.Setting.Where(item => item.EntityName == entityName && item.EntityId == entityId).ToList();
            }
            else
            {
                using var db = _tenantContextFactory.CreateDbContext();
                return db.Setting.Where(item => item.EntityName == entityName && item.EntityId == entityId).ToList();
            }
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

        public IEnumerable<string> GetEntityNames()
        {
            using var db = _tenantContextFactory.CreateDbContext();
            return db.Setting.Select(item => item.EntityName).Distinct().OrderBy(item => item).ToList();
        }
        public IEnumerable<int> GetEntityIds(string entityName)
        {
            using var db = _tenantContextFactory.CreateDbContext();
            return db.Setting.Where(item => item.EntityName == entityName)
                .Select(item => item.EntityId).Distinct().OrderBy(item => item).ToList();
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
            return EntityName == EntityNames.Host || EntityName == EntityNames.Job ||
                EntityName == EntityNames.ModuleDefinition || EntityName == EntityNames.Theme ||
                EntityName.ToLower().StartsWith("master:");
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

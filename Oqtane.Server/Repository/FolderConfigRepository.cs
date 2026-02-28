using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface IFolderConfigRepository
    {
        IEnumerable<FolderConfig> GetFolderConfigs(int siteId);

        FolderConfig GetFolderConfig(int folderConfigId);

        IDictionary<string, string> GetSettings(int folderConfigId);

        void SaveSettings(int folderConfigId, IDictionary<string, string> settings);

        FolderConfig AddFolderConfig(FolderConfig folderConfig);

        FolderConfig UpdateFolderConfig(FolderConfig folderConfig);

        void DeleteFolderConfig(int folderConfigId);
    }

    public class FolderConfigRepository : IFolderConfigRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IFolderRepository _folderRepository;
        private readonly ISettingRepository _settingRepository;

        public FolderConfigRepository(
            IDbContextFactory<TenantDBContext> dbContextFactory,
            IFolderRepository folderRepository,
            ISettingRepository settingRepository
            )
        {
            _dbContextFactory = dbContextFactory;
            _folderRepository = folderRepository;
            _settingRepository = settingRepository;
        }

        public IEnumerable<FolderConfig> GetFolderConfigs(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.FolderConfig.AsNoTracking().Where(i => i.SiteId == siteId).ToList();
        }

        public FolderConfig GetFolderConfig(int folderConfigId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.FolderConfig.AsNoTracking().FirstOrDefault(i => i.FolderConfigId == folderConfigId);
        }

        public IDictionary<string, string> GetSettings(int folderConfigId)
        {
            var settings = _settingRepository.GetSettings(EntityNames.FolderConfig, folderConfigId);
            if(settings != null)
            {
                return settings.ToDictionary(i => i.SettingName, i => i.SettingValue);
            }

            return new Dictionary<string, string>();
        }

        public void SaveSettings(int folderConfigId, IDictionary<string, string> settings)
        {
            if (settings == null)
            {
                return;
            }

            foreach (var key in settings.Keys)
            {
                if (string.IsNullOrEmpty(settings[key]))
                {
                    continue;
                }

                var setting = _settingRepository.GetSetting(EntityNames.FolderConfig, folderConfigId, key);
                if (setting == null)
                {
                    setting = new Setting
                    {
                        EntityName = EntityNames.FolderConfig,
                        EntityId = folderConfigId,
                        SettingName = key,
                        SettingValue = settings[key],
                        IsPrivate = true
                    };
                    setting = _settingRepository.AddSetting(setting);
                }
                else
                {
                    setting.SettingValue = settings[key];
                    _settingRepository.UpdateSetting(setting);
                }
            }

            //remove any settings that are not in the dictionary
            var savedSettings = _settingRepository.GetSettings(EntityNames.FolderConfig, folderConfigId);
            foreach (var savedSetting in savedSettings)
            {
                if (!settings.ContainsKey(savedSetting.SettingName))
                {
                    _settingRepository.DeleteSetting(EntityNames.FolderConfig, savedSetting.SettingId);
                }
            }
        }

        public FolderConfig AddFolderConfig(FolderConfig folderConfig)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.FolderConfig.Add(folderConfig);
            db.SaveChanges();
            return folderConfig;
        }

        public FolderConfig UpdateFolderConfig(FolderConfig folderConfig)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(folderConfig).State = EntityState.Modified;
            db.SaveChanges();
            return folderConfig;
        }

        public void DeleteFolderConfig(int folderConfigId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var folderConfig = db.FolderConfig.Find(folderConfigId);
            if (folderConfig != null)
            {
                //avoid to delete the default folder provider
                if (folderConfig.Provider == Constants.DefaultFolderProvider)
                {
                    throw new SecurityException("Cannot delete default folder provider");
                }

                //remove all the folders
                _folderRepository.GetFolders(folderConfig.SiteId)
                    .Where(i => i.FolderConfigId == folderConfigId)
                    .ToList()
                    .ForEach(item =>
                    {
                        _folderRepository.DeleteFolder(item.FolderId);
                    });

                //delete the settings

                db.FolderConfig.Remove(folderConfig);
                db.SaveChanges();
            }
        }
    }
}

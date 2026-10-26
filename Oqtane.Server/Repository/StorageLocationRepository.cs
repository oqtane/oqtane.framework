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
    public interface IStorageLocationRepository
    {
        IEnumerable<StorageLocation> GetStorageLocations(int siteId);

        StorageLocation GetStorageLocation(int StorageLocationId);

        IDictionary<string, string> GetSettings(int StorageLocationId);

        void SaveSettings(int StorageLocationId, IDictionary<string, string> settings);

        StorageLocation AddStorageLocation(StorageLocation StorageLocation);

        StorageLocation UpdateStorageLocation(StorageLocation StorageLocation);

        void DeleteStorageLocation(int StorageLocationId);
    }

    public class StorageLocationRepository : IStorageLocationRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IFolderRepository _folderRepository;
        private readonly ISettingRepository _settingRepository;

        public StorageLocationRepository(
            IDbContextFactory<TenantDBContext> dbContextFactory,
            IFolderRepository folderRepository,
            ISettingRepository settingRepository
            )
        {
            _dbContextFactory = dbContextFactory;
            _folderRepository = folderRepository;
            _settingRepository = settingRepository;
        }

        public IEnumerable<StorageLocation> GetStorageLocations(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.StorageLocation.AsNoTracking().Where(i => i.SiteId == siteId || i.SiteId == null).ToList();
        }

        public StorageLocation GetStorageLocation(int StorageLocationId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.StorageLocation.AsNoTracking().FirstOrDefault(i => i.StorageLocationId == StorageLocationId);
        }

        public IDictionary<string, string> GetSettings(int StorageLocationId)
        {
            var settings = _settingRepository.GetSettings(EntityNames.StorageLocation, StorageLocationId);
            if(settings != null)
            {
                return settings.ToDictionary(i => i.SettingName, i => i.SettingValue);
            }

            return new Dictionary<string, string>();
        }

        public void SaveSettings(int StorageLocationId, IDictionary<string, string> settings)
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

                var setting = _settingRepository.GetSetting(EntityNames.StorageLocation, StorageLocationId, key);
                if (setting == null)
                {
                    setting = new Setting
                    {
                        EntityName = EntityNames.StorageLocation,
                        EntityId = StorageLocationId,
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
            var savedSettings = _settingRepository.GetSettings(EntityNames.StorageLocation, StorageLocationId);
            foreach (var savedSetting in savedSettings)
            {
                if (!settings.ContainsKey(savedSetting.SettingName))
                {
                    _settingRepository.DeleteSetting(EntityNames.StorageLocation, savedSetting.SettingId);
                }
            }
        }

        public StorageLocation AddStorageLocation(StorageLocation StorageLocation)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.StorageLocation.Add(StorageLocation);
            db.SaveChanges();
            return StorageLocation;
        }

        public StorageLocation UpdateStorageLocation(StorageLocation StorageLocation)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(StorageLocation).State = EntityState.Modified;
            db.SaveChanges();
            return StorageLocation;
        }

        public void DeleteStorageLocation(int StorageLocationId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var StorageLocation = db.StorageLocation.Find(StorageLocationId);
            if (StorageLocation != null)
            {
                //avoid to delete the default folder provider
                if (Constants.DefaultFolderProviders.Contains(StorageLocation.Provider) || !StorageLocation.SiteId.HasValue)
                {
                    throw new SecurityException("Cannot delete default folder provider");
                }

                //remove all the folders
                _folderRepository.GetFolders(StorageLocation.SiteId.Value)
                    .Where(i => i.StorageLocationId == StorageLocationId)
                    .ToList()
                    .ForEach(item =>
                    {
                        _folderRepository.DeleteFolder(item.FolderId);
                    });

                //delete the settings

                db.StorageLocation.Remove(StorageLocation);
                db.SaveChanges();
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Security;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface IStorageLocationRepository
    {
        IEnumerable<StorageLocation> GetStorageLocations(int siteId);

        StorageLocation GetStorageLocation(int StorageLocationId);

        StorageLocation AddStorageLocation(StorageLocation StorageLocation);

        StorageLocation UpdateStorageLocation(StorageLocation StorageLocation);

        void DeleteStorageLocation(int StorageLocationId);
    }

    public class StorageLocationRepository : IStorageLocationRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly IFolderRepository _folderRepository;

        public StorageLocationRepository(
            IDbContextFactory<TenantDBContext> dbContextFactory,
            IFolderRepository folderRepository
            )
        {
            _dbContextFactory = dbContextFactory;
            _folderRepository = folderRepository;
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
                // avoid deletion of the default storage location
                if (Constants.DefaultStorageProviders.Contains(StorageLocation.Provider) || !StorageLocation.SiteId.HasValue)
                {
                    throw new SecurityException("Cannot delete default storage location");
                }

                // remove all the folders
                _folderRepository.GetFolders(StorageLocation.SiteId.Value)
                    .Where(i => i.StorageLocationId == StorageLocationId)
                    .ToList()
                    .ForEach(item =>
                    {
                        _folderRepository.DeleteFolder(item.FolderId);
                    });

                // delete the settings
                db.StorageLocation.Remove(StorageLocation);
                db.SaveChanges();
            }
        }
    }
}

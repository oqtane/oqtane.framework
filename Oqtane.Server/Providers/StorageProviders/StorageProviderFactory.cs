using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStorageLocationRepository _StorageLocationRepository;

        public StorageProviderFactory(IServiceProvider serviceProvider, IStorageLocationRepository StorageLocationRepository, IOptions<List<Models.StorageProvider>> folderProviderOptions)
        {
            _serviceProvider = serviceProvider;
            _StorageLocationRepository = StorageLocationRepository;
        }

        /// <summary>
        /// Gets the default storage location for a given site. If no default configuration exists, it creates one with the name and provider set to "PublicStorageProvider".
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetDefaultStorageLocationId(int siteId)
        {
            return GetStorageLocationId(siteId, Constants.PublicStorageProvider);
        }

        /// <summary>
        /// Gets the storage location ID for a given site. Create the storage location if it does not exist.
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetStorageLocationId(int siteId, string provider)
        {
            var storageLocation = _StorageLocationRepository.GetStorageLocations(siteId).FirstOrDefault(fp => fp.Provider == provider);
            if (storageLocation == null && Constants.DefaultStorageProviders.Contains(provider))
            {
                var folderProvider = _serviceProvider.GetServices<IStorageProvider>()?
                    .FirstOrDefault(i => GetTypeFullName(i.GetType()).Equals(provider, StringComparison.OrdinalIgnoreCase));
                storageLocation = new Models.StorageLocation
                {
                    Name = folderProvider.DisplayName,
                    Provider = provider
                };
                storageLocation = _StorageLocationRepository.AddStorageLocation(storageLocation);
            }

            return storageLocation.StorageLocationId;
        }

        public IStorageProvider GetProvider(Folder folder)
        {
            if (folder == null)
            {
                throw new ArgumentException("Storage Provider Cannot Be Created As Folder Is Null");
            }

            if (folder.StorageLocation == null)
            {
                // note that this fallback impacts performance, so it is recommended to always include the StorageLocation for the Folder
                folder.StorageLocation = _StorageLocationRepository.GetStorageLocation(folder.StorageLocationId.Value);
            }

            var folderProvider = _serviceProvider.GetServices<IStorageProvider>()?
                .FirstOrDefault(i => GetTypeFullName(i.GetType()).Equals(folder.StorageLocation.Provider, StringComparison.OrdinalIgnoreCase));

            if (folderProvider != null)
            {
                folderProvider.Initialize(folder.StorageLocationId.Value);

                return folderProvider;
            }

            return null;
        }

        private string GetTypeFullName(Type type)
        {
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
    }
}

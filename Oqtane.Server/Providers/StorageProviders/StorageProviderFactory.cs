using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class StorageProviderFactory : IStorageProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IStorageLocationRepository _StorageLocationRepository;
        private readonly ILogManager _logger;

        public StorageProviderFactory(IServiceProvider serviceProvider, IStorageLocationRepository StorageLocationRepository, IOptions<List<Models.StorageProvider>> folderProviderOptions, ILogManager logger)
        {
            _serviceProvider = serviceProvider;
            _StorageLocationRepository = StorageLocationRepository;
            _logger = logger;
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
                throw new ArgumentException("The folder is null");
            }

            if (folder.StorageLocation == null) // load the storage location manually if it is not specified
            {
                folder.StorageLocation = _StorageLocationRepository.GetStorageLocation(folder.StorageLocationId);
            }

            var folderProvider = _serviceProvider.GetServices<IStorageProvider>()?
                .FirstOrDefault(i => GetTypeFullName(i.GetType()).Equals(folder.StorageLocation.Provider, StringComparison.OrdinalIgnoreCase));

            if (folderProvider != null)
            {
                var settings = _StorageLocationRepository.GetSettings(folder.StorageLocationId);
                folderProvider.Initialize(settings);

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

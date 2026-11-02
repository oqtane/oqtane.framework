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

        public StorageProviderFactory(IServiceProvider serviceProvider, IOptions<List<Models.StorageProvider>> folderProviderOptions)
        {
            _serviceProvider = serviceProvider;
        }

        public IStorageProvider GetProvider(Folder folder)
        {
            if (folder == null)
            {
                throw new ArgumentException("Storage Provider Cannot Be Created As Folder Is Null");
            }

            var folderProvider = _serviceProvider.GetServices<IStorageProvider>()?
                .FirstOrDefault(i => GetTypeFullName(i.GetType()).Equals(folder.StorageProvider, StringComparison.OrdinalIgnoreCase));

            if (folderProvider != null)
            {
                folderProvider.Initialize(folder.FolderId);

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

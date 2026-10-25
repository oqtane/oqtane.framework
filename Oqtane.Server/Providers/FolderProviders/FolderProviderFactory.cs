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
    public class FolderProviderFactory : IFolderProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFolderConfigRepository _folderConfigRepository;
        private readonly ILogManager _logger;

        public FolderProviderFactory(IServiceProvider serviceProvider, IFolderConfigRepository folderConfigRepository, IOptions<List<Models.FolderProvider>> folderProviderOptions, ILogManager logger)
        {
            _serviceProvider = serviceProvider;
            _folderConfigRepository = folderConfigRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets the default folder configuration ID for a given site. If no default configuration exists, it creates one with the name and provider set to "PublicFolderProvider".
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetDefaultConfigId(int siteId)
        {
            return GetFolderConfigId(siteId, Constants.PublicFolderProvider);
        }

        /// <summary>
        /// Gets the folder configuration ID for a given site. create system configuration if not exists.
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int GetFolderConfigId(int siteId, string provider)
        {
            var config = _folderConfigRepository.GetFolderConfigs(siteId).FirstOrDefault(fp => fp.Provider == provider);
            if (config == null && Constants.DefaultFolderProviders.Contains(provider))
            {
                var folderProvider = _serviceProvider.GetServices<IFolderProvider>()?
                    .FirstOrDefault(i => GetTypeFullName(i.GetType()).Equals(provider, StringComparison.OrdinalIgnoreCase));
                config = new Models.FolderConfig
                {
                    Name = folderProvider.DisplayName,
                    Provider = provider
                };
                config = _folderConfigRepository.AddFolderConfig(config);
            }

            return config.FolderConfigId;
        }

        public IFolderProvider GetProvider(Folder folder)
        {
            if(folder == null)
            {
                throw new ArgumentException("The folder is null", "folder");
            }

            if(folder.FolderConfig == null) // load the folder config information manually if it hasn't been loaded
            {
                folder.FolderConfig = _folderConfigRepository.GetFolderConfig(folder.FolderConfigId);
            }

            var folderProvider = _serviceProvider.GetServices<IFolderProvider>()?
                .FirstOrDefault(i => GetTypeFullName(i.GetType()).Equals(folder.FolderConfig.Provider, StringComparison.OrdinalIgnoreCase));

            if (folderProvider != null)
            {
                var settings = _folderConfigRepository.GetSettings(folder.FolderConfigId);
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

using System;
using System.Configuration.Provider;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Interfaces;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class FolderProviderFactory : IFolderProviderFactory
    {
        private IServiceProvider _serviceProvider;
        private IFolderConfigRepository _folderConfigRepository;

        public FolderProviderFactory(IServiceProvider serviceProvider, IFolderConfigRepository folderConfigRepository)
        {
            _serviceProvider = serviceProvider;
            _folderConfigRepository = folderConfigRepository;
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
                config = new Models.FolderConfig
                {
                    Name = provider,
                    Provider = provider
                };
                config = _folderConfigRepository.AddFolderConfig(config);
            }

            return config.FolderConfigId;
        }

        public IFolderProvider GetProvider(int folderConfigId)
        {
            var folderConfig = _folderConfigRepository.GetFolderConfig(folderConfigId);
            if (folderConfig != null)
            {
                var folderProvider = _serviceProvider.GetServices<IFolderProvider>()?
                    .FirstOrDefault(i => i.Name.Equals(folderConfig.Provider, StringComparison.OrdinalIgnoreCase));
                if(folderProvider != null)
                {
                    var settings = _folderConfigRepository.GetSettings(folderConfigId);
                    folderProvider.Initialize(settings);

                    return folderProvider;
                }
            }

            return null;
        }
    }
}

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

        public int GetDefaultConfigId(int siteId)
        {
            var defaultConfig = _folderConfigRepository.GetFolderConfigs(siteId)
                .FirstOrDefault(fp => fp.Name == Constants.DefaultFolderProvider);
            if(defaultConfig == null)
            {
                defaultConfig = new Models.FolderConfig
                {
                    SiteId = siteId,
                    Name = Constants.DefaultFolderProvider,
                    Provider = Constants.DefaultFolderProvider
                };
                defaultConfig = _folderConfigRepository.AddFolderConfig(defaultConfig);
            }

            return defaultConfig.FolderConfigId;
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

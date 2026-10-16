using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class FolderProviderFactory : IFolderProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFolderConfigRepository _folderConfigRepository;
        private readonly IOptions<List<Models.FolderProvider>> _folderProviderOptions;
        private readonly ILogManager _logger;

        public FolderProviderFactory(IServiceProvider serviceProvider, IFolderConfigRepository folderConfigRepository, IOptions<List<Models.FolderProvider>> folderProviderOptions, ILogManager logger)
        {
            _serviceProvider = serviceProvider;
            _folderConfigRepository = folderConfigRepository;
            _folderProviderOptions = folderProviderOptions;
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
                try
                {
                    var folderProvider = _folderProviderOptions.Value
                    .FirstOrDefault(i => i.Type.Equals(folderConfig.Provider, StringComparison.OrdinalIgnoreCase));
                    if (folderProvider != null)
                    {
                        var settings = _folderConfigRepository.GetSettings(folderConfigId);
                        var providerType = Type.GetType(folderProvider.Type);
                        var providerInstance = ActivatorUtilities.CreateInstance(_serviceProvider, providerType) as IFolderProvider;
                        providerInstance.Initialize(settings);

                        return providerInstance;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Critical, this, Enums.LogFunction.Create, $"Error creating folder provider {folderConfig.Provider} for folder config {folderConfigId}", ex);
                }
            }

            return null;
        }
    }
}

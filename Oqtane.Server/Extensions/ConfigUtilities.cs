using System;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Shared;

namespace Oqtane.Extensions
{
    public sealed class ConfigUtilities
    {
        private static IServiceProvider _serviceProvider;

        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public static string GetConnectionStringKey()
        {
            var configManager = _serviceProvider.GetService<Oqtane.Infrastructure.IConfigManager>();
            if (configManager != null)
            {
                return configManager.GetSetting("ConnectionStringKey", SettingKeys.DefaultConnectionStringKey);
            }

            return SettingKeys.DefaultConnectionStringKey;
        }
    }
}

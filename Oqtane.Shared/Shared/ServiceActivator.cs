using System;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Shared
{
    public static class ServiceActivator
    {
        private static IServiceProvider _serviceProvider = null;

        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static IServiceScope GetScope(IServiceProvider serviceProvider = null)
        {
            var provider = serviceProvider ?? _serviceProvider;
            
            return provider?.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }
    }
}

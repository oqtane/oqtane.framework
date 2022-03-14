using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public partial class OqtaneSiteOptionsBuilder<TSiteOptions> where TSiteOptions : class, IAlias, new()
    {
        public IServiceCollection Services { get; set; }

        public OqtaneSiteOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public OqtaneSiteOptionsBuilder<TSiteOptions> AddSiteOptions<TOptions>(
            Action<TOptions, TSiteOptions> siteOptions) where TOptions : class, new()
        {
            Services.TryAddSingleton<IOptionsMonitorCache<TOptions>, SiteOptionsCache<TOptions, TSiteOptions>>();
            Services.AddSingleton<ISiteOptions<TOptions, TSiteOptions>, SiteOptions<TOptions, TSiteOptions>>
                (sp => new SiteOptions<TOptions, TSiteOptions>(siteOptions));
            Services.TryAddTransient<IOptionsFactory<TOptions>, SiteOptionsFactory<TOptions, TSiteOptions>>();
            Services.TryAddScoped<IOptionsSnapshot<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));
            Services.TryAddSingleton<IOptions<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));

            return this;
        }

        private static SiteOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp)
            where TOptions : class, new()
        {
            var cache = ActivatorUtilities.CreateInstance(sp, typeof(SiteOptionsCache<TOptions, TSiteOptions>));
            return (SiteOptionsManager<TOptions>)ActivatorUtilities.CreateInstance(sp, typeof(SiteOptionsManager<TOptions>), new[] { cache });
        }

    }
}

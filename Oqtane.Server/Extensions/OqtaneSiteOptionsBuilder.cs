using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public partial class OqtaneSiteOptionsBuilder
    {
        public IServiceCollection Services { get; set; }

        public OqtaneSiteOptionsBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public OqtaneSiteOptionsBuilder AddSiteOptions<TOptions>(
            Action<TOptions, Alias, Dictionary<string, string>> action) where TOptions : class, new()
        {
            Services.TryAddSingleton<IOptionsMonitorCache<TOptions>, SiteOptionsCache<TOptions>>();
            Services.AddSingleton<ISiteOptions<TOptions>, SiteOptions<TOptions>> (sp => new SiteOptions<TOptions>(action));
            Services.TryAddTransient<IOptionsFactory<TOptions>, SiteOptionsFactory<TOptions>>();
            Services.TryAddScoped<IOptionsSnapshot<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));
            Services.TryAddSingleton<IOptions<TOptions>>(sp => BuildOptionsManager<TOptions>(sp));

            return this;
        }

        private static SiteOptionsManager<TOptions> BuildOptionsManager<TOptions>(IServiceProvider sp)
            where TOptions : class, new()
        {
            var cache = ActivatorUtilities.CreateInstance(sp, typeof(SiteOptionsCache<TOptions>));
            return (SiteOptionsManager<TOptions>)ActivatorUtilities.CreateInstance(sp, typeof(SiteOptionsManager<TOptions>), new[] { cache });
        }

    }
}

using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteOptions<TOptions> : ISiteOptions<TOptions>
        where TOptions : class, new()
    {
        private readonly Action<TOptions, Alias, Dictionary<string, string>> configureOptions;

        public SiteOptions(Action<TOptions, Alias, Dictionary<string, string>> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(TOptions options, Alias alias, Dictionary<string, string> sitesettings)
        {
            configureOptions(options, alias, sitesettings);
        }
    }
}

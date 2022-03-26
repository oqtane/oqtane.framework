using System;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteOptions<TOptions> : ISiteOptions<TOptions>
        where TOptions : class, new()
    {
        private readonly Action<TOptions, Alias> configureOptions;

        public SiteOptions(Action<TOptions, Alias> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(TOptions options, Alias alias)
        {
            configureOptions(options, alias);
        }
    }
}

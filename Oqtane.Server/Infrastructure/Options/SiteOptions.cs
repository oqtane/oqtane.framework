using System;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteOptions<TOptions, TAlias> : ISiteOptions<TOptions, TAlias>
        where TOptions : class, new()
        where TAlias : class, IAlias, new()
    {
        private readonly Action<TOptions, TAlias> configureOptions;

        public SiteOptions(Action<TOptions, TAlias> configureOptions)
        {
            this.configureOptions = configureOptions;
        }

        public void Configure(TOptions options, TAlias siteOptions)
        {
            configureOptions(options, siteOptions);
        }
    }
}

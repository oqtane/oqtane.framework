using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteNamedOptions<TOptions> : ISiteNamedOptions<TOptions>
        where TOptions : class, new()
    {
        public string Name { get; }

        private readonly Action<TOptions, Alias, Dictionary<string, string>> configureOptions;

        public SiteNamedOptions(string name, Action<TOptions, Alias, Dictionary<string, string>> configureOptions)
        {
            Name = name;
            this.configureOptions = configureOptions;
        }

        public void Configure(string name, TOptions options, Alias alias, Dictionary<string, string> sitesettings)
        {
            if (name == Name)
            {
                configureOptions(options, alias, sitesettings);
            }
        }
    }
}

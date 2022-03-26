using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Oqtane.Infrastructure
{
    public class SiteOptionsFactory<TOptions> : IOptionsFactory<TOptions>
        where TOptions : class, new()
    {
        private readonly IConfigureOptions<TOptions>[] _configureOptions;
        private readonly IPostConfigureOptions<TOptions>[] _postConfigureOptions;
        private readonly ISiteOptions<TOptions>[] _siteOptions;
        private readonly IAliasAccessor _aliasAccessor;

        public SiteOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> configureOptions, IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions, IEnumerable<ISiteOptions<TOptions>> siteOptions, IAliasAccessor aliasAccessor)
        {
            _configureOptions = configureOptions as IConfigureOptions<TOptions>[] ?? new List<IConfigureOptions<TOptions>>(configureOptions).ToArray();
            _postConfigureOptions = postConfigureOptions as IPostConfigureOptions<TOptions>[] ?? new List<IPostConfigureOptions<TOptions>>(postConfigureOptions).ToArray();
            _siteOptions = siteOptions as ISiteOptions<TOptions>[] ?? new List<ISiteOptions<TOptions>>(siteOptions).ToArray();
            _aliasAccessor = aliasAccessor;
         }

        public TOptions Create(string name)
        {
            // default options
            var options = new TOptions();
            foreach (var setup in _configureOptions)
            {
                if (setup is IConfigureNamedOptions<TOptions> namedSetup)
                {
                    namedSetup.Configure(name, options);
                }
                else if (name == Options.DefaultName)
                {
                    setup.Configure(options);
                }
            }

            // override with site specific options
            if (_aliasAccessor?.Alias != null)
            {
                foreach (var siteOption in _siteOptions)
                {
                    siteOption.Configure(options, _aliasAccessor.Alias);
                }
            }

            // post configuration
            foreach (var post in _postConfigureOptions)
            {
                post.PostConfigure(name, options);
            }

            return options;
        }
    }
}

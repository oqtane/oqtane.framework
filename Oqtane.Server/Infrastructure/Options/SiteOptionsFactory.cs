using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Oqtane.Extensions;

namespace Oqtane.Infrastructure
{
    public class SiteOptionsFactory<TOptions> : IOptionsFactory<TOptions>
        where TOptions : class, new()
    {
        private readonly IConfigureOptions<TOptions>[] _configureOptions;
        private readonly IPostConfigureOptions<TOptions>[] _postConfigureOptions;
        private readonly ISiteOptions<TOptions>[] _siteOptions;
        private readonly IHttpContextAccessor _accessor;

        public SiteOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> configureOptions, IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions, IEnumerable<ISiteOptions<TOptions>> siteOptions, IHttpContextAccessor accessor)
        {
            _configureOptions = configureOptions as IConfigureOptions<TOptions>[] ?? new List<IConfigureOptions<TOptions>>(configureOptions).ToArray();
            _postConfigureOptions = postConfigureOptions as IPostConfigureOptions<TOptions>[] ?? new List<IPostConfigureOptions<TOptions>>(postConfigureOptions).ToArray();
            _siteOptions = siteOptions as ISiteOptions<TOptions>[] ?? new List<ISiteOptions<TOptions>>(siteOptions).ToArray();
            _accessor = accessor;
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
            if (_accessor.HttpContext?.GetAlias() != null && _accessor.HttpContext?.GetSiteSettings() != null)
            {
                foreach (var siteOption in _siteOptions)
                {
                    siteOption.Configure(options, _accessor.HttpContext.GetAlias(), _accessor.HttpContext.GetSiteSettings());
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

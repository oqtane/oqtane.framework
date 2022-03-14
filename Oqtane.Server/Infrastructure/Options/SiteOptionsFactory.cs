using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class SiteOptionsFactory<TOptions, TAlias> : IOptionsFactory<TOptions>
        where TOptions : class, new()
        where TAlias : class, IAlias, new()
    {
        private readonly IConfigureOptions<TOptions>[] _configureOptions;
        private readonly IPostConfigureOptions<TOptions>[] _postConfigureOptions;
        private readonly IValidateOptions<TOptions>[] _validations;
        private readonly ISiteOptions<TOptions, TAlias>[] _siteOptions;
        private readonly IAliasAccessor _aliasAccessor;

        public SiteOptionsFactory(IEnumerable<IConfigureOptions<TOptions>> configureOptions, IEnumerable<IPostConfigureOptions<TOptions>> postConfigureOptions, IEnumerable<IValidateOptions<TOptions>> validations, IEnumerable<ISiteOptions<TOptions, TAlias>> siteOptions, IAliasAccessor aliasAccessor)
        {
            _configureOptions = configureOptions as IConfigureOptions<TOptions>[] ?? new List<IConfigureOptions<TOptions>>(configureOptions).ToArray();
            _postConfigureOptions = postConfigureOptions as IPostConfigureOptions<TOptions>[] ?? new List<IPostConfigureOptions<TOptions>>(postConfigureOptions).ToArray();
            _validations = validations as IValidateOptions<TOptions>[] ?? new List<IValidateOptions<TOptions>>(validations).ToArray();
            _siteOptions = siteOptions as ISiteOptions<TOptions, TAlias>[] ?? new List<ISiteOptions<TOptions, TAlias>>(siteOptions).ToArray();
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
                    siteOption.Configure(options, _aliasAccessor.Alias as TAlias);
                }
            }

            // post configuration
            foreach (var post in _postConfigureOptions)
            {
                post.PostConfigure(name, options);
            }

            //if (_validations.Length > 0)
            //{
            //    var failures = new List<string>();
            //    foreach (IValidateOptions<TOptions> validate in _validations)
            //    {
            //        ValidateOptionsResult result = validate.Validate(name, options);
            //        if (result != null && result.Failed)
            //        {
            //            failures.AddRange(result.Failures);
            //        }
            //    }
            //    if (failures.Count > 0)
            //    {
            //        throw new OptionsValidationException(name, typeof(TOptions), failures);
            //    }
            //}

            return options;
        }
    }
}

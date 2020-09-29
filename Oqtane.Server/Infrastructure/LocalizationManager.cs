using System.Collections;
using Microsoft.Extensions.Options;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class LocalizationManager : ILocalizationManager
    {
        private static readonly string DefaultCulture = Constants.DefaultCulture;
        private static readonly string[] SupportedCultures = new[] { DefaultCulture };

        private readonly LocalizationOptions _localizationOptions;

        public LocalizationManager(IOptions<LocalizationOptions> localizationOptions)
        {
            _localizationOptions = localizationOptions.Value;
        }

        public string GetDefaultCulture()
            => string.IsNullOrEmpty(_localizationOptions.DefaultCulture)
                ? DefaultCulture
                : _localizationOptions.DefaultCulture;

        public string[] GetSupportedCultures()
            => _localizationOptions.SupportedCultures.IsNullOrEmpty()
                ? SupportedCultures
                : _localizationOptions.SupportedCultures;
    }
}

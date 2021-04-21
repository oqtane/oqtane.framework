using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class LocalizationManager : ILocalizationManager
    {
        private static readonly string DefaultCulture = Constants.DefaultCulture;
        private static readonly string[] DefaultSupportedCultures = new[] { DefaultCulture };

        private readonly LocalizationOptions _localizationOptions;

        public LocalizationManager(IOptions<LocalizationOptions> localizationOptions)
        {
            _localizationOptions = localizationOptions.Value;
        }

        public string GetDefaultCulture()
            => String.IsNullOrEmpty(_localizationOptions.DefaultCulture)
                ? DefaultCulture
                : _localizationOptions.DefaultCulture;

        public string[] GetSupportedCultures()
        {
            var supportedCultures = GetCulturesNamesFromSatelliteAssemblies();
            if (supportedCultures.IsNullOrEmpty())
            {
                return DefaultSupportedCultures;
            }
            else
            {
                return supportedCultures;
            }
        }

        private static string[] GetCulturesNamesFromSatelliteAssemblies()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            return cultures
                .Where(c => !c.Equals(CultureInfo.InvariantCulture) && Directory.Exists(Path.Combine(assemblyPath, c.Name)))
                .Select(c => c.Name)
                .Union(DefaultSupportedCultures)
                .OrderBy(c => c)
                .ToArray();
        }
    }
}

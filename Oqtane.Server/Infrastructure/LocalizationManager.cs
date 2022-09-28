using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class LocalizationManager : ILocalizationManager
    {
        private static readonly string DefaultCulture = Constants.DefaultCulture;

        private readonly LocalizationOptions _localizationOptions;

        public LocalizationManager(IOptions<LocalizationOptions> localizationOptions)
        {
            _localizationOptions = localizationOptions.Value;
        }

        public string GetDefaultCulture()
        {
            if (string.IsNullOrEmpty(_localizationOptions.DefaultCulture))
            {
                return DefaultCulture;
            }
            else
            {
                return _localizationOptions.DefaultCulture;
            }
        }

        public string[] GetSupportedCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures).Select(item => item.Name).OrderBy(c => c).ToArray();
        }

        public string[] GetInstalledCultures()
        {
            var cultures = new List<string>();
            foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"{Constants.ClientId}{Constants.SatelliteAssemblyExtension}", SearchOption.AllDirectories))
            {
                cultures.Add(Path.GetFileName(Path.GetDirectoryName(file)));
            }
            return cultures.OrderBy(c => c).ToArray();
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        { 
            List<string> cultures = new List<string>();
            foreach(var file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Oqtane.Client.resources.dll", SearchOption.AllDirectories))
            {
                cultures.Add(Path.GetFileName(Path.GetDirectoryName(file)));
            }
            if (cultures.Count == 0)
            {
                return SupportedCultures;
            }
            else
            {
                return cultures.ToArray();
            }
        }
    }
}

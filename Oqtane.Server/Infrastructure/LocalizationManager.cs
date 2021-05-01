using System;
using System.Collections.Generic;
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
            var cultures = new List<string>(DefaultSupportedCultures);
            foreach(var file in Directory.EnumerateFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Oqtane.Client.resources.dll", SearchOption.AllDirectories))
            {
                cultures.Add(Path.GetFileName(Path.GetDirectoryName(file)));
            }

            return cultures.OrderBy(c => c).ToArray();
        }
    }
}

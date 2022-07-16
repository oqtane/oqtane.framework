using System;
using System.Collections.Generic;
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
    }
}

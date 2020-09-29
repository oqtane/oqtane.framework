using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Infrastructure.Localization
{
    public static class LocalizationSettings
    {
        static LocalizationSettings()
        {
            DefaultCulture = Constants.DefaultCulture;
            SupportedCultures = new List<string> { DefaultCulture };
        }

        public static string DefaultCulture { get; set; }

        public static List<string> SupportedCultures { get; }
    }
}

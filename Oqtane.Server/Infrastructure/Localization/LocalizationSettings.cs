using System.Collections.Generic;

namespace Oqtane.Infrastructure.Localization
{
    public static class LocalizationSettings
    {
        private const string EnglishCulture = "en-US";

        static LocalizationSettings()
        {
            DefaultCulture = EnglishCulture;
            SupportedCultures = new List<string> { DefaultCulture };
        }

        public static string DefaultCulture { get; set; }

        public static IList<string> SupportedCultures { get; set; }
    }
}

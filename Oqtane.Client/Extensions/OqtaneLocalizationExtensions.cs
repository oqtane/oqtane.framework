namespace Microsoft.Extensions.Localization
{
    public static class OqtaneLocalizationExtensions
    {
        /// <summary>
        /// Gets the string resource for the specified key and returns the value if the resource does not exist
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="key">the static key used to identify the string resource</param>
        /// <param name="value">the default value if the resource for the static key does not exist</param>
        /// <returns></returns>
        public static string GetString(this IStringLocalizer localizer, string key, string value)
        {
            string localizedValue = localizer[key];
            if (localizedValue == key && !string.IsNullOrEmpty(value)) // not localized
            {
                localizedValue = value;
            }
            return localizedValue;
        }
    }
}

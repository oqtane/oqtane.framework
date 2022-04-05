using System.Collections.Generic;

namespace Oqtane.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue, bool nullOrEmptyValueIsValid = false)
        {
            if (dictionary != null && key != null && dictionary.ContainsKey(key))
            {
                if (nullOrEmptyValueIsValid || (dictionary[key] != null && !string.IsNullOrEmpty(dictionary[key].ToString())))
                {
                    return dictionary[key];
                }
            }
            return defaultValue;
        }
    }
}

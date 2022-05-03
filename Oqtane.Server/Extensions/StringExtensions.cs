using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Extensions
{
    public static class StringExtensions
    {
        public static bool StartWithAnyOf(this string s, IEnumerable<string> list)
        {
            if (s == null)
            {
                return false;
            }

            return list.Any(f => s.StartsWith(f));
        }

        public static string ReplaceMultiple(this string s, string[] oldValues, string newValue)
        {
            foreach(string value in oldValues)
            {
                s = s.Replace(value, newValue);
            }
            return s;
        }
    }
}

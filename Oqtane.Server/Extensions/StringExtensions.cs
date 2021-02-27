using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using Oqtane.Models;

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
    }
}

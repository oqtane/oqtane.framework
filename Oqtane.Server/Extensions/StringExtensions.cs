using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;

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

        public static string GetMimeType(this string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }
    }
}

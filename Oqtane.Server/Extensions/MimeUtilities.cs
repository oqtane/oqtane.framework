using Microsoft.AspNetCore.StaticFiles;
using Oqtane.Models;

namespace Oqtane.Extensions
{
    public static class MimeUtilities
    {
        /// <summary>
        ///     Return Mime content type based on file extension
        /// </summary>
        /// <param name="fileName">File name</param>
        public static string GetMimeType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(fileName, out var contentType))
                contentType = "application/octet-stream";
            // we can add additional mappings here

            return contentType;
        }

        /// <summary>
        ///     Return Mime content type based on file extension
        /// </summary>
        public static string GetMimeType(this File file)
        {
            return GetMimeType(file?.Name);
        }
    }
}

using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using File = Oqtane.Models.File;

namespace Oqtane.Shared
{
    public static class Utilities
    {
        public static string ToModuleDefinitionName(this Type type)
        {
            if (type == null) return null;
            var assemblyFullName = type.Assembly.FullName;
            var assemblyName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(",", StringComparison.Ordinal));
            return $"{type.Namespace}, {assemblyName}";
        }

        public static (string UrlParameters, string Querystring, string Anchor) ParseParameters(string parameters)
        {
            // /urlparameters /urlparameters?Id=1 /urlparameters#5 /urlparameters?Id=1#5 /urlparameters?reload#5

            // Id=1 Id=1#5 reload#5 reload

            // #5

            var urlparameters = string.Empty;
            var querystring = string.Empty;
            var anchor = string.Empty;

            if (parameters.Contains('#'))
            {
                anchor = parameters.Split('#').Last();
                parameters = parameters.Replace("#" + anchor, "");
            }

            if (parameters.Contains('?'))
            {
                urlparameters = parameters.Split('?').First();
                querystring = parameters.Replace(urlparameters + "?", "");
            }
            else if (parameters.Contains('/'))
            {
                urlparameters = parameters;
            }
            else
            {
                querystring = parameters;
            }

            return (urlparameters, querystring, anchor);
        }

        public static string NavigateUrl(string alias, string path, string parameters)
        {
            string urlparameters;
            string querystring;
            string anchor;

            // parse parameters
            (urlparameters, querystring, anchor) = ParseParameters(parameters);
            if (!string.IsNullOrEmpty(urlparameters))
            {
                if (urlparameters.StartsWith("/")) urlparameters = urlparameters.Remove(0, 1);
                path += $"/{Constants.UrlParametersDelimiter}/{urlparameters}";
            }

            // build url
            var uriBuilder = new UriBuilder
            {
                Path = !string.IsNullOrEmpty(alias)
                    ? (!string.IsNullOrEmpty(path))
                        ? $"{alias}/{path}"
                        : $"{alias}"
                    : $"{path}",
                Query = querystring,
            };

            anchor = string.IsNullOrEmpty(anchor) ? "" : "#" + anchor;
            return uriBuilder.Uri.PathAndQuery + anchor;
        }

        public static string EditUrl(string alias, string path, int moduleid, string action, string parameters)
        {
            if (moduleid != -1)
            {
                path += $"/{Constants.ModuleDelimiter}/{moduleid}";
                if (!string.IsNullOrEmpty(action))
                {
                    path += $"/{action}";
                }
            }

            return NavigateUrl(alias, path, parameters);
        }

        public static string FileUrl(Alias alias, string folderpath, string filename)
        {
            return FileUrl(alias, folderpath, filename, false);
        }

        public static string FileUrl(Alias alias, string folderpath, string filename, bool download)
        {
            var aliasUrl = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path : "";
            var querystring = (download) ? "?download" : "";
            return $"{alias?.BaseUrl}{aliasUrl}{Constants.FileUrl}{folderpath.Replace("\\", "/")}{filename}{querystring}";
        }

        public static string FileUrl(Alias alias, int fileid)
        {
            return FileUrl(alias, fileid, false);
        }

        public static string FileUrl(Alias alias, int fileid, bool download)
        {
            var aliasUrl = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path : "";
            var querystring = (download) ? "?download" : "";
            return $"{alias?.BaseUrl}{aliasUrl}{Constants.FileUrl}id/{fileid}{querystring}";
        }

        public static string ImageUrl(Alias alias, int fileId, int width, int height, string mode)
        {
            return ImageUrl(alias, fileId, width, height, mode, "", "", 0, false);
        }

        public static string ImageUrl(Alias alias, int fileId, int width, int height, string mode, string position, string background, int rotate, bool recreate)
        {
            var url = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path : "";
            mode = string.IsNullOrEmpty(mode) ? "crop" : mode;
            position = string.IsNullOrEmpty(position) ? "center" : position;
            background = string.IsNullOrEmpty(background) ? "000000" : background;
            return $"{alias?.BaseUrl}{url}{Constants.ImageUrl}{fileId}/{width}/{height}/{mode}/{position}/{background}/{rotate}/{recreate}";
        }

        public static string TenantUrl(Alias alias, string url)
        {
            url = (!url.StartsWith("/")) ? "/" + url : url;
            url = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path + url : url;
            return $"{alias?.BaseUrl}{url}";
        }

        public static string AddUrlParameters(params object[] parameters)
        {
            var url = "";
            for (var i = 0; i < parameters.Length; i++)
            {
                url += "/" + parameters[i].ToString();
            }
            return url;
        }

        public static string FormatContent(string content, Alias alias, string operation)
        {
            var aliasUrl = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path : "";
            switch (operation)
            {
                case "save":
                    content = content.Replace(alias?.BaseUrl + aliasUrl + Constants.FileUrl, Constants.FileUrl);
                    // legacy
                    content = content.Replace(UrlCombine("Content", "Tenants", alias.TenantId.ToString(), "Sites", alias.SiteId.ToString()), "[siteroot]");
                    content = content.Replace(alias.Path + Constants.ContentUrl, Constants.ContentUrl);
                    break;
                case "render":
                    content = content.Replace(Constants.FileUrl, alias?.BaseUrl + aliasUrl + Constants.FileUrl);
                    // legacy
                    content = content.Replace("[siteroot]", UrlCombine("Content", "Tenants", alias.TenantId.ToString(), "Sites", alias.SiteId.ToString()));
                    content = content.Replace(Constants.ContentUrl, alias.Path + Constants.ContentUrl);
                    break;
            }
            return content;
        }

        public static string GetTypeName(string fullyqualifiedtypename)
        {
            if (fullyqualifiedtypename.Contains(","))
            {
                return fullyqualifiedtypename.Substring(0, fullyqualifiedtypename.IndexOf(","));
            }
            else
            {
                return fullyqualifiedtypename;
            }
        }

        public static string GetFullTypeName(string fullyqualifiedtypename)
        {
            if (fullyqualifiedtypename.Contains(", Version="))
            {
                return fullyqualifiedtypename.Substring(0, fullyqualifiedtypename.IndexOf(", Version="));
            }
            else
            {
                return fullyqualifiedtypename;
            }
        }

        public static string GetAssemblyName(string fullyqualifiedtypename)
        {
            fullyqualifiedtypename = GetFullTypeName(fullyqualifiedtypename);
            if (fullyqualifiedtypename.Contains(","))
            {
                return fullyqualifiedtypename.Substring(fullyqualifiedtypename.IndexOf(",") + 1).Trim();
            }
            else
            {
                return "";
            }
        }

        public static string GetTypeNameLastSegment(string typename, int segment)
        {
            if (typename.Contains(","))
            {
                // remove assembly if fully qualified type
                typename = typename.Substring(0, typename.IndexOf(","));
            }

            // segment 0 is the last segment, segment 1 is the second to last segment, etc...
            string[] segments = typename.Split('.');
            string name = "";
            if (segment < segments.Length)
            {
                name = segments[segments.Length - (segment + 1)];
            }

            return name;
        }

        public static string GetFriendlyUrl(string text)
        {
            string result = "";
            if (text != null)
            {
                var normalizedString = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();
                var stringLength = normalizedString.Length;
                var prevdash = false;
                char c;
                for (int i = 0; i < stringLength; i++)
                {
                    c = normalizedString[i];
                    switch (CharUnicodeInfo.GetUnicodeCategory(c))
                    {
                        case UnicodeCategory.LowercaseLetter:
                        case UnicodeCategory.UppercaseLetter:
                        case UnicodeCategory.DecimalDigitNumber:
                            if (c < 128)
                                stringBuilder.Append(c);
                            else
                                stringBuilder.Append(RemapInternationalCharToAscii(c));
                            prevdash = false;
                            break;

                        case UnicodeCategory.SpaceSeparator:
                        case UnicodeCategory.ConnectorPunctuation:
                        case UnicodeCategory.DashPunctuation:
                        case UnicodeCategory.OtherPunctuation:
                        case UnicodeCategory.MathSymbol:
                            if (!prevdash)
                            {
                                stringBuilder.Append('-');
                                prevdash = true;
                            }

                            break;
                    }
                }

                result = stringBuilder.ToString().Trim('-');
            }

            return result;
        }

        private static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåą".Contains(s))
            {
                return "a";
            }
            else if ("èéêëę".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïı".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøőð".Contains(s))
            {
                return "o";
            }
            else if ("ùúûüŭů".Contains(s))
            {
                return "u";
            }
            else if ("çćčĉ".Contains(s))
            {
                return "c";
            }
            else if ("żźž".Contains(s))
            {
                return "z";
            }
            else if ("śşšŝ".Contains(s))
            {
                return "s";
            }
            else if ("ñń".Contains(s))
            {
                return "n";
            }
            else if ("ýÿ".Contains(s))
            {
                return "y";
            }
            else if ("ğĝ".Contains(s))
            {
                return "g";
            }
            else if (c == 'ř')
            {
                return "r";
            }
            else if (c == 'ł')
            {
                return "l";
            }
            else if (c == 'đ')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'þ')
            {
                return "th";
            }
            else if (c == 'ĥ')
            {
                return "h";
            }
            else if (c == 'ĵ')
            {
                return "j";
            }
            else
            {
                return "";
            }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            return Regex.IsMatch(email,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
        }

        public static string PathCombine(params string[] segments)
        {
            var separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

            for (int i = 1; i < segments.Length; i++)
            {
                if (Path.IsPathRooted(segments[i]))
                {
                    segments[i] = segments[i].TrimStart(separators);
                    if (String.IsNullOrEmpty(segments[i]))
                    {
                        segments[i] = " ";
                    }
                }
            }

            return Path.Combine(segments).TrimEnd();
        }

        public static string UrlCombine(params string[] segments)
{
            segments = segments.Where(item => !string.IsNullOrEmpty(item) && item != "/" && item != "\\").ToArray();
            for (int i = 1; i < segments.Length; i++)
            {
                segments[i] = segments[i].Replace("\\", "/");
            }
            return string.Join("/", segments);
        }

        public static bool IsPathValid(this Folder folder)
        {
            return IsPathOrFileValid(folder.Name);
        }

        public static bool IsFileValid(this File file)
        {
            return IsPathOrFileValid(file.Name);
        }

        public static bool IsPathOrFileValid(this string name)
        {
            return (name != null &&
                    name.IndexOfAny(Constants.InvalidFileNameChars) == -1 &&
                    !Constants.InvalidFileNameEndingChars.Any(name.EndsWith) &&
                    !Constants.ReservedDevices.Split(',').Contains(name.ToUpper().Split('.')[0]));
        }

        public static bool TryGetQueryValue(
            this Uri uri,
            string key,
            out string value,
            string defaultValue = null)
        {
            value = defaultValue;
            string query = uri.Query;
            return !string.IsNullOrEmpty(query) && Utilities.ParseQueryString(query).TryGetValue(key, out value);
        }

        public static bool TryGetQueryValueInt(
            this Uri uri,
            string key,
            out int value,
            int defaultValue = 0)
        {
            value = defaultValue;
            string s;
            return uri.TryGetQueryValue(key, out s, (string)null) && int.TryParse(s, out value);
        }

        public static Dictionary<string, string> ParseQueryString(string query)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query))
            {
                query = query.Substring(1);
                string str = query;
                char[] separator = new char[1] { '&' };
                foreach (string key in str.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (key != "")
                    {
                        if (key.Contains("="))
                        {
                            string[] strArray = key.Split('=', StringSplitOptions.None);
                            dictionary.Add(strArray[0], strArray[1]);
                        }
                        else
                            dictionary.Add(key, "true");
                    }
                }
            }

            return dictionary;
        }

        public static string LogMessage(object @class, string message)
        {
            return $"[{@class.GetType()}] {message}";
        }

        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, string time, TimeZoneInfo localTimeZone = null)
        {
            localTimeZone ??= TimeZoneInfo.Local;
            if (date != null)
            {
                if (!string.IsNullOrEmpty(time))
                {
                    return TimeZoneInfo.ConvertTime(DateTime.Parse(date.Value.Date.ToShortDateString() + " " + time), localTimeZone, TimeZoneInfo.Utc);
                }
                return TimeZoneInfo.ConvertTime(date.Value.Date, localTimeZone, TimeZoneInfo.Utc);
            }
            return null;
        }

        public static (DateTime? date, string time) UtcAsLocalDateAndTime(DateTime? dateTime, TimeZoneInfo timeZone = null)
        {
            timeZone ??= TimeZoneInfo.Local;
            DateTime? localDateTime = null;
            string localTime = string.Empty;

            if (dateTime.HasValue && dateTime?.Kind != DateTimeKind.Local)
            {
                if (dateTime?.Kind == DateTimeKind.Unspecified)
                {
                    // Treat Unspecified as Utc not Local. This is due to EF Core, on some databases, after retrieval will have DateTimeKind as Unspecified.
                    // All values in database should be UTC.
                    // Normal .net conversion treats Unspecified as local.
                    // https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo.converttime?view=net-6.0
                    localDateTime = TimeZoneInfo.ConvertTime(new DateTime(dateTime.Value.Ticks, DateTimeKind.Utc), timeZone);
                }
                else
                {
                    localDateTime = TimeZoneInfo.ConvertTime(dateTime.Value, timeZone);
                }
            }

            if (localDateTime != null && localDateTime.Value.TimeOfDay.TotalSeconds != 0)
            {
                localTime = localDateTime.Value.ToString("HH:mm");
            }

            return (localDateTime?.Date, localTime);
        }

        [Obsolete("ContentUrl(Alias alias, int fileId) is deprecated. Use FileUrl(Alias alias, int fileId) instead.", false)]
        public static string ContentUrl(Alias alias, int fileId)
        {
            return ContentUrl(alias, fileId, false);
        }

        [Obsolete("ContentUrl(Alias alias, int fileId, bool asAttachment) is deprecated. Use FileUrl(Alias alias, int fileId, bool download) instead.", false)]
        public static string ContentUrl(Alias alias, int fileId, bool asAttachment)
        {
            var aliasUrl = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path : "";
            var method = asAttachment ? "/attach" : "";

            return $"{alias?.BaseUrl}{aliasUrl}{Constants.ContentUrl}{fileId}{method}";
        }
    }
}

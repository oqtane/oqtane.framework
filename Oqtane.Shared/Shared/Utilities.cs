using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using NodaTime;
using NodaTime.Extensions;

using Oqtane.Models;

using File = Oqtane.Models.File;
using TimeZone = Oqtane.Models.TimeZone;

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

        public static (string UrlParameters, string Querystring, string Fragment) ParseParameters(string parameters)
        {
            // /urlparameters /urlparameters?id=1 /urlparameters#5 /urlparameters?id=1#5 /urlparameters?reload#5
            // ?id=1 ?id=1#5 ?reload#5 ?reload
            // id=1 id=1#5 reload#5 reload
            // #5

            // create absolute url to convert to Uri
            parameters = (!parameters.StartsWith("/") && !parameters.StartsWith("#") && !parameters.StartsWith("?") ? "?" : "") + parameters;
            parameters = Constants.PackageRegistryUrl + parameters;
            var uri = new Uri(parameters);
            var querystring = uri.Query.Replace("?", "");
            var fragment = uri.Fragment.Replace("#", "");
            var urlparameters = uri.LocalPath;
            urlparameters = (urlparameters == "/") ? "" : urlparameters;

            return (urlparameters, querystring, fragment);
        }

        public static string NavigateUrl(string alias, string path, string parameters)
        {
            string querystring = "";
            string fragment = "";

            if (!string.IsNullOrEmpty(path) && !path.StartsWith("/")) path = "/" + path;

            if (!string.IsNullOrEmpty(parameters))
            {
                (string urlparameters, querystring, fragment) = ParseParameters(parameters);
                if (!string.IsNullOrEmpty(urlparameters))
                {
                    path += (path.EndsWith("/") ? "" : "/") + $"{Constants.UrlParametersDelimiter}/{urlparameters.Substring(1)}";
                }
            }

            // build url
            var uriBuilder = new UriBuilder
            {
                Path = !string.IsNullOrEmpty(alias)
                    ? (!string.IsNullOrEmpty(path)) ? $"{alias}{path}": $"{alias}"
                    : $"{path}",
                Query = querystring,
                Fragment = fragment
            };

            return uriBuilder.Uri.PathAndQuery;
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
            background = string.IsNullOrEmpty(background) ? "transparent" : background;
            return $"{alias?.BaseUrl}{url}{Constants.ImageUrl}{fileId}/{width}/{height}/{mode}/{position}/{background}/{rotate}/{recreate}";
        }

        public static string ImageUrl(Alias alias, string folderpath, string filename, int width, int height, string mode, string position, string background, int rotate, string format, bool recreate)
        {
            var aliasUrl = (alias != null && !string.IsNullOrEmpty(alias.Path)) ? "/" + alias.Path : "";
            mode = string.IsNullOrEmpty(mode) ? "crop" : mode;
            position = string.IsNullOrEmpty(position) ? "center" : position;
            background = string.IsNullOrEmpty(background) ? "transparent" : background;
            format = string.IsNullOrEmpty(format) ? "png" : format;
            var querystring = $"?width={width}&height={height}&mode={mode}&position={position}&background={background}&rotate={rotate}&format={format}&recreate={recreate}";
            return $"{alias?.BaseUrl}{aliasUrl}{Constants.FileUrl}{folderpath.Replace("\\", "/")}{filename}{querystring}";
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
            if (string.IsNullOrEmpty(content) || alias == null)
                return content;

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
                    content = content.Replace("[wwwroot]", alias?.BaseUrl + aliasUrl + "/");
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

        public static string GetFriendlyUrl(string url)
        {
            string result = "";
            if (url != null)
            {
                var normalizedString = WebUtility.UrlDecode(url).ToLowerInvariant().Normalize(NormalizationForm.FormD);
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
            var url = new List<string>();
            for (int i = 0; i < segments.Length; i++)
            {
                segments[i] = segments[i].Replace("\\", "/");
                if (!string.IsNullOrEmpty(segments[i]) && segments[i] != "/")
                {
                    foreach (var segment in segments[i].Split('/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        url.Add(segment);
                    }
                }
            }
            return string.Join("/", url);
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
            Dictionary<string, string> querystring = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // case insensistive keys
            if (!string.IsNullOrEmpty(query))
            {
                if (query.StartsWith("?"))
                {
                    query = query.Substring(1); // ignore "?"
                }
                foreach (string kvp in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (kvp != "")
                    {
                        if (kvp.Contains("="))
                        {
                            string[] pair = kvp.Split('=');
                            if (!querystring.ContainsKey(pair[0]))
                            {
                                querystring.Add(pair[0], pair[1]);
                            }
                        }
                        else
                        {
                            if (!querystring.ContainsKey(kvp))
                            {
                                querystring.Add(kvp, "true"); // default parameter when no value is provided
                            }
                        }
                    }
                }
            }
            return querystring;
        }

        public static string CreateQueryString(Dictionary<string, string> parameters)
        {
            var querystring = "";
            if (parameters.Count > 0)
            {
                foreach (var kvp in parameters)
                {
                    querystring += (querystring == "") ? "?" : "&";
                    querystring += kvp.Key + "=" + kvp.Value;
                }
            }
            return querystring;
        }

        public static string GetUrlPath(string url)
        {
            if (url.Contains("?"))
            {
                url = url.Substring(0, url.IndexOf("?"));
            }
            return url;
        }

        public static string LogMessage(object @class, string message)
        {
            return $"[{@class.GetType()}] {message}";
        }

        //Time conversions with TimeZoneInfo
        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, string time, TimeZoneInfo localTimeZone = null)
        {
            if (date != null && !string.IsNullOrEmpty(time) && TimeSpan.TryParse(time, out TimeSpan timespan))
            {
                return LocalDateAndTimeAsUtc(date.Value.Date.Add(timespan), localTimeZone);
            }
            return null;
        }

        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, DateTime? time, TimeZoneInfo localTimeZone = null)
        {
            if (date != null)
            {
                if (time != null)
                {
                    return LocalDateAndTimeAsUtc(date.Value.Date.Add(time.Value.TimeOfDay), localTimeZone);
                }
                return LocalDateAndTimeAsUtc(date.Value.Date, localTimeZone);
            }
            return null;
        }

        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, TimeZoneInfo localTimeZone = null)
        {
            if (date != null)
            {
                localTimeZone ??= TimeZoneInfo.Local;
                return TimeZoneInfo.ConvertTime(date.Value, localTimeZone, TimeZoneInfo.Utc);
            }
            return null;
        }

        public static DateTime? UtcAsLocalDate(DateTime? dateTime, TimeZoneInfo timeZone = null)
        {
            return UtcAsLocalDateAndTime(dateTime, timeZone).date;
        }

        public static DateTime? UtcAsLocalDateTime(DateTime? dateTime, TimeZoneInfo timeZone = null)
        {
            var result = UtcAsLocalDateAndTime(dateTime, timeZone);
            if (result.date != null && !string.IsNullOrEmpty(result.time) && TimeSpan.TryParse(result.time, out TimeSpan timespan))
            {
                result.date = result.date.Value.Add(timespan);
            }
            return result.date;
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

        //Time conversions with NodaTime (IANA) timezoneId
        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, string time, string localTimeZoneId)
        {
            if (date != null && !string.IsNullOrEmpty(time) && TimeSpan.TryParse(time, out TimeSpan timespan))
            {
                return LocalDateAndTimeAsUtc(date.Value.Date.Add(timespan), localTimeZoneId);
            }
            return null;
        }

        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, DateTime? time, string localTimeZoneId)
        {
            if (date != null)
            {
                if (time != null)
                {
                    return LocalDateAndTimeAsUtc(date.Value.Date.Add(time.Value.TimeOfDay), localTimeZoneId);
                }
                return LocalDateAndTimeAsUtc(date.Value.Date, localTimeZoneId);
            }
            return null;
        }

        public static DateTime? LocalDateAndTimeAsUtc(DateTime? date, string localTimeZoneId)
        {
            if (date != null)
            {
                DateTimeZone localTimeZone;

                if (!string.IsNullOrEmpty(localTimeZoneId))
                {
                    localTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(localTimeZoneId) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault();
                }
                else
                {
                    localTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
                }

                var localDateTime = LocalDateTime.FromDateTime(date.Value);
                return localTimeZone.AtLeniently(localDateTime).ToDateTimeUtc();
            }
            return null;
        }

        public static DateTime? UtcAsLocalDate(DateTime? dateTime, string timeZoneId)
        {
            return UtcAsLocalDateAndTime(dateTime, timeZoneId).date;
        }

        public static DateTime? UtcAsLocalDateTime(DateTime? dateTime, string timeZoneId)
        {
            var result = UtcAsLocalDateAndTime(dateTime, timeZoneId);
            if (result.date != null && !string.IsNullOrEmpty(result.time) && TimeSpan.TryParse(result.time, out TimeSpan timespan))
            {
                result.date = result.date.Value.Add(timespan);
            }
            return result.date;
        }

        public static (DateTime? date, string time) UtcAsLocalDateAndTime(DateTime? dateTime, string timeZoneId)
        {
            DateTimeZone localTimeZone;

            if (!string.IsNullOrEmpty(timeZoneId))
            {
                localTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId) ?? DateTimeZoneProviders.Tzdb.GetSystemDefault();
            }
            else
            {
                localTimeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            }

            DateTime? localDateTime = null;
            string localTime = string.Empty;

            if (dateTime.HasValue && dateTime?.Kind != DateTimeKind.Local)
            {
                Instant instant;

                if (dateTime?.Kind == DateTimeKind.Unspecified)
                {
                    // Treat Unspecified as Utc not Local. This is due to EF Core, on some databases, after retrieval will have DateTimeKind as Unspecified.
                    // All values in database should be UTC.
                    // Normal .net conversion treats Unspecified as local.
                    // https://docs.microsoft.com/en-us/dotnet/api/system.timezoneinfo.converttime?view=net-6.0
                    instant = Instant.FromDateTimeUtc(new DateTime(dateTime.Value.Ticks, DateTimeKind.Utc));
                }
                else
                {
                    instant = Instant.FromDateTimeUtc(dateTime.Value);
                }

                localDateTime = instant.InZone(localTimeZone).ToDateTimeOffset().DateTime;
            }

            if (localDateTime != null && localDateTime.Value.TimeOfDay.TotalSeconds != 0)
            {
                localTime = localDateTime.Value.ToString("HH:mm");
            }

            return (localDateTime?.Date, localTime);
        }

        public static bool IsEffectiveAndNotExpired(DateTime? effectiveDate, DateTime? expiryDate)
        {
            DateTime currentUtcTime = DateTime.UtcNow;

            if (effectiveDate.HasValue && expiryDate.HasValue)
            {
                return currentUtcTime >= effectiveDate.Value && currentUtcTime <= expiryDate.Value;
            }
            else if (effectiveDate.HasValue)
            {
                return currentUtcTime >= effectiveDate.Value;
            }
            else if (expiryDate.HasValue)
            {
                return currentUtcTime <= expiryDate.Value;
            }
            else
            {
                return true;
            }
        }

        public static bool ValidateEffectiveExpiryDates(DateTime? effectiveDate, DateTime? expiryDate)
        {
            effectiveDate ??= DateTime.MinValue;
            expiryDate ??= DateTime.MinValue;

            if (effectiveDate != DateTime.MinValue && expiryDate != DateTime.MinValue)
            {
                return effectiveDate <= expiryDate;
            }
            else if (effectiveDate != DateTime.MinValue)
            {
                return true;
            }
            else if (expiryDate != DateTime.MinValue)
            {
                return true;
            }
            else
            {
                return true;
            }
        }

        public static string GenerateSimpleHash(string text)
        {
            unchecked // prevent overflow exception
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash.ToString("X8");
            }
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

        [Obsolete("IsPageModuleVisible(DateTime?, DateTime?) is deprecated. Use IsEffectiveAndNotExpired(DateTime?, DateTime?) instead.", false)]
        public static bool IsPageModuleVisible(DateTime? effectiveDate, DateTime? expiryDate)
        {
            return IsEffectiveAndNotExpired(effectiveDate, expiryDate);
        }

    }
}

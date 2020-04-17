using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Oqtane.Shared
{
    public static class Utilities
    {
        public static string ToModuleDefinitionName(this Type type)
        {
            if (type == null) return null;
            var assemblyFullName = type.Assembly.FullName;
            var assemblyName = assemblyFullName.Substring(0,  assemblyFullName.IndexOf(",", StringComparison.Ordinal));
            return $"{type.Namespace}, {assemblyName}";
        }

        public static string NavigateUrl(string alias, string path, string parameters)
        {
            var uriBuilder = new UriBuilder
            {
                Path = !string.IsNullOrEmpty(alias)
                ? (!string.IsNullOrEmpty(path)) 
                    ? $"{alias}/{path}" 
                    : $"{alias}"
                : $"{path}",
                Query = parameters
            };

            return uriBuilder.Uri.PathAndQuery;
        }

        public static string EditUrl(string alias, string path, int moduleid, string action, string parameters)
        {
            if (moduleid != -1)
            {
                path += $"/{moduleid}";
                if (!string.IsNullOrEmpty(action))
                {
                    path += $"/{action}";
                }
            }
            return NavigateUrl(alias, path, parameters);
        }

        public static string ContentUrl(string alias, int fileid)
        {
            string url = (alias == "") ? "/~" : alias;
            url += Constants.ContentUrl + fileid.ToString();
            return url;
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
    }
}

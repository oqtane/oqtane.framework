using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace Oqtane.Shared
{
    public sealed class SearchUtils
    {
        private const string PunctuationMatch = "[~!#\\$%\\^&*\\(\\)-+=\\{\\[\\}\\]\\|;:\\x22'<,>\\.\\?\\\\\\t\\r\\v\\f\\n]";
        private static readonly Regex _stripWhiteSpaceRegex = new Regex("\\s+", RegexOptions.Compiled);
        private static readonly Regex _stripTagsRegex = new Regex("<[^<>]*>", RegexOptions.Compiled);
        private static readonly Regex _afterRegEx = new Regex(PunctuationMatch + "\\s", RegexOptions.Compiled);
        private static readonly Regex _beforeRegEx = new Regex("\\s" + PunctuationMatch, RegexOptions.Compiled);

        public static string Clean(string html, bool removePunctuation)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            if (html.Contains("&lt;"))
            {
                html = WebUtility.HtmlDecode(html);
            }

            html = StripTags(html, true);
            html = WebUtility.HtmlDecode(html);

            if (removePunctuation)
            {
                html = StripPunctuation(html, true);
                html = StripWhiteSpace(html, true);
            }

            return html;
        }

        public static IList<string> GetKeywordsList(string keywords)
        {
            var keywordsList = new List<string>();
            if(!string.IsNullOrEmpty(keywords))
            {
                foreach (var keyword in keywords.Split(' '))
                {
                    if (!string.IsNullOrWhiteSpace(keyword.Trim()))
                    {
                        keywordsList.Add(keyword.Trim());
                    }
                }
            }

            return keywordsList;
        }

        private static string StripTags(string html, bool retainSpace)
        {
            return _stripTagsRegex.Replace(html, retainSpace ? " " : string.Empty);
        }

        private static string StripPunctuation(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            string retHTML = html + " ";

            var repString = retainSpace ? " " : string.Empty;
            while (_beforeRegEx.IsMatch(retHTML))
            {
                retHTML = _beforeRegEx.Replace(retHTML, repString);
            }

            while (_afterRegEx.IsMatch(retHTML))
            {
                retHTML = _afterRegEx.Replace(retHTML, repString);
            }

            return retHTML.Trim('"');
        }

        private static string StripWhiteSpace(string html, bool retainSpace)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            return _stripWhiteSpaceRegex.Replace(html, retainSpace ? " " : string.Empty);
        }
    }
}

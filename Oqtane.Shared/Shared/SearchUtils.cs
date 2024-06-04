using System.Collections.Generic;

namespace Oqtane.Shared
{
    public sealed class SearchUtils
    {
        private static readonly IList<string> _systemPages;

        static SearchUtils()
        {
            _systemPages = new List<string> { "login", "register", "profile", "404", "search" };
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

        public static bool IsSystemPage(Models.Page page)
        {
            return page.Path.Contains("admin") || _systemPages.Contains(page.Path);
        }
    }
}

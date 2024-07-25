using System.Collections.Generic;

namespace Oqtane.Shared
{
    public sealed class SearchUtils
    {
        public static List<string> GetKeywords(string keywords)
        {
            var keywordsList = new List<string>();
            if(!string.IsNullOrEmpty(keywords))
            {
                foreach (var keyword in keywords.Split(' '))
                {
                    if (!string.IsNullOrWhiteSpace(keyword.Trim()))
                    {
                        keywordsList.Add(keyword.Trim().ToLower());
                    }
                }
            }

            return keywordsList;
        }
    }
}

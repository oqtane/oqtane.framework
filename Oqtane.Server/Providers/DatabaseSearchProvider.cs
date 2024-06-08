using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class DatabaseSearchProvider : ISearchProvider
    {
        private readonly ISearchContentRepository _searchContentRepository;

        private const string IgnoreWords = "the,be,to,of,and,a,i,in,that,have,it,for,not,on,with,he,as,you,do,at,this,but,his,by,from,they,we,say,her,she,or,an,will,my,one,all,would,there,their,what,so,up,out,if,about,who,get,which,go,me,when,make,can,like,time,no,just,him,know,take,people,into,year,your,good,some,could,them,see,other,than,then,now,look,only,come,its,over,think,also,back,after,use,two,how,our,work,first,well,way,even,new,want,because,any,these,give,day,most,us";
        private const int WordMinLength = 3;
        public string Name => Constants.DefaultSearchProviderName;

        public DatabaseSearchProvider(ISearchContentRepository searchContentRepository)
        {
            _searchContentRepository = searchContentRepository;
        }

        public void Commit()
        {
        }

        public void DeleteSearchContent(string id)
        {
            _searchContentRepository.DeleteSearchContent(id);
        }

        public bool Optimize()
        {
            return true;
        }

        public void ResetIndex()
        {
            _searchContentRepository.DeleteAllSearchContent();
        }

        public void SaveSearchContent(SearchContent searchContent, bool autoCommit = false)
        {
            //remove exist document
            _searchContentRepository.DeleteSearchContent(searchContent.EntityName, searchContent.EntityId);

            //clean the search content to remove html tags
            CleanSearchContent(searchContent);

            _searchContentRepository.AddSearchContent(searchContent);

            //save the index words
            AnalyzeSearchContent(searchContent);
        }

        public async Task<SearchResults> SearchAsync(SearchQuery searchQuery, Func<SearchContent, SearchQuery, bool> validateFunc)
        {
            var totalResults = 0;

            var searchContentList = await _searchContentRepository.GetSearchContentsAsync(searchQuery);

            //convert the search content to search results.
            var results = searchContentList
                .Where(i => validateFunc(i, searchQuery))
                .Select(i => ConvertToSearchResult(i, searchQuery));

            if (searchQuery.SortDirection == SearchSortDirections.Descending)
            {
                switch (searchQuery.SortField)
                {
                    case SearchSortFields.Relevance:
                        results = results.OrderByDescending(i => i.Score).ThenByDescending(i => i.ModifiedTime);
                        break;
                    case SearchSortFields.Title:
                        results = results.OrderByDescending(i => i.Title).ThenByDescending(i => i.ModifiedTime);
                        break;
                    default:
                        results = results.OrderByDescending(i => i.ModifiedTime);
                        break;
                }
            }
            else
            {
                switch (searchQuery.SortField)
                {
                    case SearchSortFields.Relevance:
                        results = results.OrderBy(i => i.Score).ThenByDescending(i => i.ModifiedTime);
                        break;
                    case SearchSortFields.Title:
                        results = results.OrderBy(i => i.Title).ThenByDescending(i => i.ModifiedTime);
                        break;
                    default:
                        results = results.OrderBy(i => i.ModifiedTime);
                        break;
                }
            }

            //remove duplicated results based on page id for Page and Module types
            results = results.DistinctBy(i =>
            {
                if (i.EntityName == EntityNames.Page || i.EntityName == EntityNames.Module)
                {
                    var pageId = i.SearchContentProperties.FirstOrDefault(p => p.Name == Constants.SearchPageIdPropertyName)?.Value ?? string.Empty;
                    return !string.IsNullOrEmpty(pageId) ? pageId : i.UniqueKey;
                }
                else
                {
                    return i.UniqueKey;
                }
            });

            totalResults = results.Count();

            return new SearchResults
            {
                Results = results.Skip(searchQuery.PageIndex * searchQuery.PageSize).Take(searchQuery.PageSize).ToList(),
                TotalResults = totalResults
            };
        }

        private SearchResult ConvertToSearchResult(SearchContent searchContent, SearchQuery searchQuery)
        {
            var searchResult = new SearchResult()
            {
                SearchContentId = searchContent.SearchContentId,
                SiteId = searchContent.SiteId,
                EntityName = searchContent.EntityName,
                EntityId = searchContent.EntityId,
                Title = searchContent.Title,
                Description = searchContent.Description,
                Body = searchContent.Body,
                Url = searchContent.Url,
                ModifiedTime = searchContent.ModifiedTime,
                SearchContentProperties = searchContent.SearchContentProperties,
                Snippet = BuildSnippet(searchContent, searchQuery),
                Score = CalculateScore(searchContent, searchQuery)
            };

            return searchResult;
        }

        private float CalculateScore(SearchContent searchContent, SearchQuery searchQuery)
        {
            var score = 0f;
            foreach (var keyword in SearchUtils.GetKeywordsList(searchQuery.Keywords))
            {
                score += searchContent.SearchContentWords.Where(i => i.SearchWord.Word.StartsWith(keyword)).Sum(i => i.Count);
            }

            return score / 100;
        }

        private string BuildSnippet(SearchContent searchContent, SearchQuery searchQuery)
        {
            var content = $"{searchContent.Title} {searchContent.Description} {searchContent.Body}";
            var snippet = string.Empty;
            foreach (var keyword in SearchUtils.GetKeywordsList(searchQuery.Keywords))
            {
                if (!string.IsNullOrWhiteSpace(keyword) && content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    var start = content.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) - 20;
                    var prefix = "...";
                    var suffix = "...";
                    if (start <= 0)
                    {
                        start = 0;
                        prefix = string.Empty;
                    }

                    var length = searchQuery.BodySnippetLength;
                    if (start + length >= content.Length)
                    {
                        length = content.Length - start;
                        suffix = string.Empty;
                    }

                    snippet = $"{prefix}{content.Substring(start, length)}{suffix}";
                    break;


                }
            }

            if (string.IsNullOrEmpty(snippet))
            {
                snippet = content.Substring(0, searchQuery.BodySnippetLength);
            }

            foreach (var keyword in SearchUtils.GetKeywordsList(searchQuery.Keywords))
            {
                snippet = Regex.Replace(snippet, $"({keyword})", $"<b>$1</b>", RegexOptions.IgnoreCase);
            }

            return snippet;
        }

        private void AnalyzeSearchContent(SearchContent searchContent)
        {
            //analyze the search content and save the index words
            var indexContent = $"{searchContent.Title} {searchContent.Description} {searchContent.Body} {searchContent.AdditionalContent}";
            var words = GetWords(indexContent, WordMinLength);
            var existingSearchContentWords = _searchContentRepository.GetSearchContentWords(searchContent.SearchContentId);
            foreach (var kvp in words)
            {
                var searchContentWord = existingSearchContentWords.FirstOrDefault(i => i.SearchWord.Word == kvp.Key);
                if (searchContentWord != null)
                {
                    searchContentWord.Count = kvp.Value;
                    searchContentWord.ModifiedOn = DateTime.UtcNow;
                    _searchContentRepository.UpdateSearchContentWord(searchContentWord);
                }
                else
                {
                    var searchWord = _searchContentRepository.GetSearchWord(kvp.Key);
                    if (searchWord == null)
                    {
                        searchWord = _searchContentRepository.AddSearchWord(new SearchWord { Word = kvp.Key, CreatedOn = DateTime.UtcNow });
                    }

                    searchContentWord = new SearchContentWord
                    {
                        SearchContentId = searchContent.SearchContentId,
                        SearchWordId = searchWord.SearchWordId,
                        Count = kvp.Value,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedOn = DateTime.UtcNow
                    };

                    _searchContentRepository.AddSearchContentWord(searchContentWord);
                }
            }
        }

        private static Dictionary<string, int> GetWords(string content, int minLength)
        {
            content = FormatText(content);

            var words = new Dictionary<string, int>();
            var ignoreWords = IgnoreWords.Split(',');

            if (!string.IsNullOrEmpty(content))
            {
                foreach (var word in content.Split(' '))
                {
                    if (word.Length >= minLength && !ignoreWords.Contains(word))
                    {
                        if (!words.ContainsKey(word))
                        {
                            words.Add(word, 1);
                        }
                        else
                        {
                            words[word] += 1;
                        }
                    }
                }
            }

            return words;
        }

        private static string FormatText(string text)
        {
            text = HtmlEntity.DeEntitize(text);
            foreach (var punctuation in ".?!,;:-_()[]{}'\"/\\".ToCharArray())
            {
                text = text.Replace(punctuation, ' ');
            }
            text = text.Replace("  ", " ").ToLower().Trim();

            return text;
        }

        private void CleanSearchContent(SearchContent searchContent)
        {
            searchContent.Title = GetCleanContent(searchContent.Title);
            searchContent.Description = GetCleanContent(searchContent.Description);
            searchContent.Body = GetCleanContent(searchContent.Body);
            searchContent.AdditionalContent = GetCleanContent(searchContent.AdditionalContent);
        }

        private string GetCleanContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return string.Empty;
            }

            content = WebUtility.HtmlDecode(content);

            var page = new HtmlDocument();
            page.LoadHtml(content);

            var phrases = page.DocumentNode.Descendants().Where(i =>
                    i.NodeType == HtmlNodeType.Text &&
                    i.ParentNode.Name != "script" &&
                    i.ParentNode.Name != "style" &&
                    !string.IsNullOrEmpty(i.InnerText.Trim())
                ).Select(i => i.InnerText);

            return string.Join(" ", phrases);
        }
    }
}

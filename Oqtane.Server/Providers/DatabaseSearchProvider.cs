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

        private const string SearchIgnoreWordsSetting = "Search_IgnoreWords";
        private const string SearchMinimumWordLengthSetting = "Search_MininumWordLength";
        private const string IgnoreWords = "the,be,to,of,and,a,i,in,that,have,it,for,not,on,with,he,as,you,do,at,this,but,his,by,from,they,we,say,her,she,or,an,will,my,one,all,would,there,their,what,so,up,out,if,about,who,get,which,go,me,when,make,can,like,time,no,just,him,know,take,people,into,year,your,good,some,could,them,see,other,than,then,now,look,only,come,its,over,think,also,back,after,use,two,how,our,work,first,well,way,even,new,want,because,any,these,give,day,most,us";
        private const int MinimumWordLength = 3;

        public string Name => Constants.DefaultSearchProviderName;

        public DatabaseSearchProvider(ISearchContentRepository searchContentRepository)
        {
            _searchContentRepository = searchContentRepository;
        }

        public async Task<List<SearchResult>> GetSearchResultsAsync(SearchQuery searchQuery)
        {
            var searchContents = await _searchContentRepository.GetSearchContentsAsync(searchQuery);
            return searchContents.Select(item => ConvertToSearchResult(item, searchQuery)).ToList();
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
                Url = Utilities.TenantUrl(searchQuery.Alias, searchContent.Url),
                Permissions = searchContent.Permissions,
                ContentModifiedBy = searchContent.ContentModifiedBy,
                ContentModifiedOn = searchContent.ContentModifiedOn,
                SearchContentProperties = searchContent.SearchContentProperties,
                Snippet = BuildSnippet(searchContent, searchQuery),
                Score = CalculateScore(searchContent, searchQuery)
            };

            return searchResult;
        }

        private string BuildSnippet(SearchContent searchContent, SearchQuery searchQuery)
        {
            var content = $"{searchContent.Title} {searchContent.Description} {searchContent.Body}";
            var snippet = string.Empty;
            foreach (var keyword in SearchUtils.GetKeywords(searchQuery.Keywords))
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

                    var length = searchQuery.BodyLength;
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
                snippet = content.Substring(0, searchQuery.BodyLength);
            }

            foreach (var keyword in SearchUtils.GetKeywords(searchQuery.Keywords))
            {
                snippet = Regex.Replace(snippet, $"({keyword})", $"<b>$1</b>", RegexOptions.IgnoreCase);
            }

            return snippet;
        }

        private float CalculateScore(SearchContent searchContent, SearchQuery searchQuery)
        {
            var score = 0f;
            foreach (var keyword in SearchUtils.GetKeywords(searchQuery.Keywords))
            {
                score += searchContent.SearchContentWords.Where(i => i.SearchWord.Word.StartsWith(keyword)).Sum(i => i.Count);
            }

            return score / 100;
        }

        public Task SaveSearchContent(SearchContent searchContent, Dictionary<string, string> siteSettings)
        {
            // remove existing search content
            _searchContentRepository.DeleteSearchContent(searchContent.EntityName, searchContent.EntityId);

            if (!searchContent.IsDeleted)
            {
                // clean the search content to remove html tags
                CleanSearchContent(searchContent);

                _searchContentRepository.AddSearchContent(searchContent);

                // save the index words
                AnalyzeSearchContent(searchContent, siteSettings);
            }

            return Task.CompletedTask;
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

        private void AnalyzeSearchContent(SearchContent searchContent, Dictionary<string, string> siteSettings)
        {
            var ignoreWords = IgnoreWords.Split(',');
            if (siteSettings.ContainsKey(SearchIgnoreWordsSetting) && !string.IsNullOrEmpty(siteSettings[SearchIgnoreWordsSetting]))
            {
                ignoreWords = siteSettings[SearchIgnoreWordsSetting].Split(',');
            }
            var minimumWordLength = MinimumWordLength;
            if (siteSettings.ContainsKey(SearchMinimumWordLengthSetting) && !string.IsNullOrEmpty(siteSettings[SearchMinimumWordLengthSetting]))
            {
                minimumWordLength = int.Parse(siteSettings[SearchMinimumWordLengthSetting]);
            }

            // analyze the search content and save the index words
            var indexContent = $"{searchContent.Title} {searchContent.Description} {searchContent.Body} {searchContent.AdditionalContent}";
            var words = GetWords(indexContent, ignoreWords, minimumWordLength);

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

        private static Dictionary<string, int> GetWords(string content, string[] ignoreWords, int minimumWordLength)
        {
            content = FormatContent(content);

            var words = new Dictionary<string, int>();

            if (!string.IsNullOrEmpty(content))
            {
                foreach (var term in content.Split(' '))
                {
                    var word = term.ToLower().Trim();
                    if (word.Length >= minimumWordLength && !ignoreWords.Contains(word))
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

        private static string FormatContent(string text)
        {
            text = HtmlEntity.DeEntitize(text);
            foreach (var punctuation in ".?!,;:_()[]{}'\"/\\".ToCharArray())
            {
                text = text.Replace(punctuation, ' ');
            }
            return text;
        }

        public Task ResetIndex()
        {
            _searchContentRepository.DeleteAllSearchContent();
            return Task.CompletedTask;
        }
    }
}

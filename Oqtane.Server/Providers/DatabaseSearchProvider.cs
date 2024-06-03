using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Services;
using Oqtane.Shared;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Oqtane.Providers
{
    public class DatabaseSearchProvider : ISearchProvider
    {
        private readonly ISearchDocumentRepository _searchDocumentRepository;

        private const float TitleBoost = 100f;
        private const float DescriptionBoost = 10f;
        private const float BodyBoost = 10f;
        private const float AdditionalContentBoost = 5f;

        public string Name => Constants.DefaultSearchProviderName;

        public DatabaseSearchProvider(ISearchDocumentRepository searchDocumentRepository)
        {
            _searchDocumentRepository = searchDocumentRepository;
        }

        public void Commit()
        {
        }

        public void DeleteDocument(string id)
        {
            _searchDocumentRepository.DeleteSearchDocument(id);
        }

        public bool Optimize()
        {
            return true;
        }

        public void ResetIndex()
        {
            _searchDocumentRepository.DeleteAllSearchDocuments();
        }

        public void SaveDocument(SearchDocument document, bool autoCommit = false)
        {
            //remove exist document
            _searchDocumentRepository.DeleteSearchDocument(document.IndexerName, document.EntryId);

            _searchDocumentRepository.AddSearchDocument(document);
        }

        public async Task<SearchResults> SearchAsync(SearchQuery searchQuery, Func<SearchDocument, SearchQuery, bool> validateFunc)
        {
            var totalResults = 0;

            var documents = await _searchDocumentRepository.GetSearchDocumentsAsync(searchQuery);

            //convert the search documents to search results.
            var results = documents
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
                if (i.IndexerName == Constants.PageSearchIndexManagerName || i.IndexerName == Constants.ModuleSearchIndexManagerName)
                {
                    var pageId = i.Properties.FirstOrDefault(p => p.Name == Constants.SearchPageIdPropertyName)?.Value ?? string.Empty;
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

        private SearchResult ConvertToSearchResult(SearchDocument searchDocument, SearchQuery searchQuery)
        {
            var searchResult = new SearchResult()
            {
                SearchDocumentId = searchDocument.SearchDocumentId,
                SiteId = searchDocument.SiteId,
                IndexerName = searchDocument.IndexerName,
                EntryId = searchDocument.EntryId,
                Title = searchDocument.Title,
                Description = searchDocument.Description,
                Body = searchDocument.Body,
                Url = searchDocument.Url,
                ModifiedTime = searchDocument.ModifiedTime,
                Tags = searchDocument.Tags,
                Properties = searchDocument.Properties,
                Snippet = BuildSnippet(searchDocument, searchQuery),
                Score = CalculateScore(searchDocument, searchQuery)
            };

            return searchResult;
        }

        private float CalculateScore(SearchDocument searchDocument, SearchQuery searchQuery)
        {
            var score = 0f;
            foreach (var keyword in SearchUtils.GetKeywordsList(searchQuery.Keywords))
            {
                score += Regex.Matches(searchDocument.Title, keyword, RegexOptions.IgnoreCase).Count * TitleBoost;
                score += Regex.Matches(searchDocument.Description, keyword, RegexOptions.IgnoreCase).Count * DescriptionBoost;
                score += Regex.Matches(searchDocument.Body, keyword, RegexOptions.IgnoreCase).Count * BodyBoost;
                score += Regex.Matches(searchDocument.AdditionalContent, keyword, RegexOptions.IgnoreCase).Count * AdditionalContentBoost;
            }

            return score / 100;
        }

        private string BuildSnippet(SearchDocument searchDocument, SearchQuery searchQuery)
        {
            var content = $"{searchDocument.Title} {searchDocument.Description} {searchDocument.Body}";
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
    }
}

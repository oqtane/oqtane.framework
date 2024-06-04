using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISearchContentRepository
    {
        Task<IEnumerable<SearchContent>> GetSearchContentListAsync(SearchQuery searchQuery);
        SearchContent AddSearchContent(SearchContent searchContent);
        void DeleteSearchContent(int searchContentId);
        void DeleteSearchContent(string entityName, int entryId);
        void DeleteSearchContent(string uniqueKey);
        void DeleteAllSearchContent();

        SearchContentWordSource GetSearchContentWordSource(string word);
        SearchContentWordSource AddSearchContentWordSource(SearchContentWordSource wordSource);

        IEnumerable<SearchContentWords> GetWords(int searchContentId);
        SearchContentWords AddSearchContentWords(SearchContentWords word);
        SearchContentWords UpdateSearchContentWords(SearchContentWords word);
    }
}

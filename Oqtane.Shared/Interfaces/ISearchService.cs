using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchService
    {
        Task<SearchResults> GetSearchResultsAsync(SearchQuery searchQuery);

        Task<string> SaveSearchContentsAsync(List<SearchContent> searchContents, Dictionary<string, string> siteSettings);

        Task DeleteSearchContentsAsync(int siteId);
    }
}

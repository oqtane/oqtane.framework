using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchProvider
    {
        string Name { get; }

        Task<List<SearchResult>> GetSearchResultsAsync(SearchQuery searchQuery);

        Task SaveSearchContent(SearchContent searchContent, Dictionary<string, string> siteSettings);

        Task DeleteSearchContent(int siteId);
    }
}

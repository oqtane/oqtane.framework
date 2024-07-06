using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchService
    {
        Task SaveSearchContentAsync(List<SearchContent> searchContents);

        Task<SearchResults> SearchAsync(SearchQuery searchQuery);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchProvider
    {
        string Name { get; }

        Task SaveSearchContent(SearchContent searchContent, bool autoCommit = false);

        Task DeleteSearchContent(string id);

        Task<SearchResults> SearchAsync(SearchQuery searchQuery, Func<SearchContent, SearchQuery, bool> validateFunc);
        
        Task<bool> Optimize();

        Task Commit();

        Task ResetIndex();
    }
}

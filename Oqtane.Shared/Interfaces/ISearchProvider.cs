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

        void SaveSearchContent(SearchContent searchContent, bool autoCommit = false);

        void DeleteSearchContent(string id);

        Task<SearchResults> SearchAsync(SearchQuery searchQuery, Func<SearchContent, SearchQuery, bool> validateFunc);
        
        bool Optimize();

        void Commit();

        void ResetIndex();
    }
}

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

        void SaveDocument(SearchDocument document, bool autoCommit = false);

        void DeleteDocument(string id);

        Task<SearchResults> SearchAsync(SearchQuery searchQuery, Func<SearchDocument, SearchQuery, bool> validateFunc);
        
        bool Optimize();

        void Commit();

        void ResetIndex();
    }
}

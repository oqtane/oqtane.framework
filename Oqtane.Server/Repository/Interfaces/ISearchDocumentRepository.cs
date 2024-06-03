using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISearchDocumentRepository
    {
        Task<IEnumerable<SearchDocument>> GetSearchDocumentsAsync(SearchQuery searchQuery);
        SearchDocument AddSearchDocument(SearchDocument searchDocument);
        void DeleteSearchDocument(int searchDocumentId);
        void DeleteSearchDocument(string indexerName, int entryId);
        void DeleteSearchDocument(string uniqueKey);
        void DeleteAllSearchDocuments();
    }
}

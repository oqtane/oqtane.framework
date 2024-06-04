using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchService
    {
        void IndexContent(int siteId, DateTime? startTime, Action<string> logNote, Action<string> handleError);

        Task<SearchResults> SearchAsync(SearchQuery searchQuery);
    }
}

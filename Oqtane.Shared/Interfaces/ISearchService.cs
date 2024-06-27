using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchService
    {
        Task IndexContent(int siteId, DateTime? startTime, Func<string, Task> logNote, Func<string, Task> handleError);

        Task<SearchResults> SearchAsync(SearchQuery searchQuery);
    }
}

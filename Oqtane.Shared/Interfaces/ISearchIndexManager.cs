using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchIndexManager
    {
        int Priority { get; }

        string Name { get; }

        bool IsIndexEnabled(int siteId);

        Task<int> IndexContent(int siteId, DateTime? startTime, Func<List<SearchContent>, Task> processSearchContent, Func<string, Task> handleError);
    }
}

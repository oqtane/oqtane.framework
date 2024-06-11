using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchIndexManager
    {
        int Priority { get; }

        string Name { get; }

        bool IsIndexEnabled(int siteId);

        int IndexContent(int siteId, DateTime? startTime, Action<List<SearchContent>> processSearchContent, Action<string> handleError);
    }
}

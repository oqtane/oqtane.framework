using System;
using Oqtane.Models;

namespace Oqtane.Services
{
    public interface ISearchResultManager
    {
        string Name { get; }

        bool Visible(SearchContent searchResult, SearchQuery searchQuery);

        string GetUrl(SearchResult searchResult, SearchQuery searchQuery);
    }
}

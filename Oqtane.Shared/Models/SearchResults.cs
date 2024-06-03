using System.Collections.Generic;

namespace Oqtane.Models
{
    public class SearchResults
    {
        public IList<SearchResult> Results { get; set; }

        public int TotalResults { get; set; }
    }
}

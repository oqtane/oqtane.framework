using System.Collections.Generic;

namespace Oqtane.Models
{
    public class SearchResults
    {
        public List<SearchResult> Results { get; set; }

        public int TotalResults { get; set; }
    }
}

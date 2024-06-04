namespace Oqtane.Models
{
    public class SearchResult : SearchContent
    {
        public float Score { get; set; }

        public string DisplayScore { get; set; }

        public string Snippet { get; set; }
    }
}

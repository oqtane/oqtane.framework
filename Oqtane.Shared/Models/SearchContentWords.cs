using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class SearchContentWords
    {
        [Key]
        public int WordId { get; set; }

        public int SearchContentId { get; set; }

        public int WordSourceId { get; set; }

        public int Count { get; set; }

        public SearchContentWordSource WordSource { get; set; }
    }
}

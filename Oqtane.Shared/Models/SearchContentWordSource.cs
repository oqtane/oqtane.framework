using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class SearchContentWordSource
    {
        [Key]
        public int WordSourceId { get; set; }

        public string Word { get; set; }
    }
}

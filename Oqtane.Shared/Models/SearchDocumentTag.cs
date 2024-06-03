using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class SearchDocumentTag
    {
        [Key]
        public int TagId { get; set; }

        public int SearchDocumentId { get; set; }

        public string Tag { get; set; }
    }
}

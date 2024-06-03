using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Models
{
    public class SearchDocumentProperty
    {
        [Key]
        public int PropertyId { get; set; }

        public int SearchDocumentId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}

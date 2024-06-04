using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Models
{
    public class SearchContentProperty
    {
        [Key]
        public int PropertyId { get; set; }

        public int SearchContentId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}

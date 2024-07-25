using System;
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class SearchWord
    {
        [Key]
        public int SearchWordId { get; set; }

        public string Word { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class SearchContentWord
    {
        [Key]
        public int SearchContentWordId { get; set; }

        public int SearchContentId { get; set; }

        public int SearchWordId { get; set; }

        public int Count { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public SearchWord SearchWord { get; set; }
    }
}

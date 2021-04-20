using System;
using Oqtane.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Modules.HtmlText.Models
{
    public class HtmlText : IAuditable
    {
        [Key]
        public int HtmlTextId { get; set; }
        public int ModuleId { get; set; }
        public string Content { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Shared.Modules.HtmlText.Models
{
    [Table("HtmlText")]
    public class HtmlTextInfo
    {
        [Key]
        public int HtmlTextId { get; set; }
        public int ModuleId { get; set; }
        public string Content { get; set; }
    }
}

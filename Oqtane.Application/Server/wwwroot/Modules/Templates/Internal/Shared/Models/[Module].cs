using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace [Owner].Module.[Module].Models
{
    [Table("[Owner][Module]")]
    public class [Module] : ModelBase
    {
        [Key]
        public int [Module]Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models.[Module]s
{
    public class [Module] : IAuditable
    {
        [Key]
        public int [Module]Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

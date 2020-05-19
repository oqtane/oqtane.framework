using System;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace [Owner].[Module]s.Models
{
    [Table("[Owner][Module]")]
    public class [Module] : IAuditable
    {
        public int [Module]Id { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

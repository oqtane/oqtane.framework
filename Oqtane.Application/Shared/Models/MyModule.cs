using System;
using System.ComponentModel.DataAnnotations;
using Oqtane.Models;

namespace Oqtane.Application.Models
{
    public class MyModule : IAuditable
    {
        [Key]
        public int MyModuleId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

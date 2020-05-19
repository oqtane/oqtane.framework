using System;
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class Role : IAuditable
    {
        public int RoleId { get; set; }
        public int? SiteId { get; set; }
        [Required]
        [StringLength(maximumLength: 256, MinimumLength = 1)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Description { get; set; }
        public bool IsAutoAssigned { get; set; }
        public bool IsSystem { get; set; }


        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

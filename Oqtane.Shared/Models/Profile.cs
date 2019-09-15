using System;

namespace Oqtane.Models
{
    public class Profile : IAuditable
    {
        public int ProfileId { get; set; }
        public int? SiteId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int ViewOrder { get; set; }
        public int MaxLength { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public bool IsPrivate { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

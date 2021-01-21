using System;

namespace Oqtane.Models
{
    public class Language : IAuditable
    {
        public int LanguageId { get; set; }

        public int? SiteId { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public bool IsDefault { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedOn { get; set; }
    }
}

using System;

namespace Oqtane.Models
{
    public class PageModule : IAuditable, IDeletable
    {
        public int PageModuleId { get; set; }
        public int PageId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Pane { get; set; }
        public int Order { get; set; }
        public string ContainerType { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }


        public Module Module { get; set; }
    }
}

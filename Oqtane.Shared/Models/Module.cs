using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Module : IAuditable
    {
        public int ModuleId { get; set; }
        public int SiteId { get; set; }
        public string ModuleDefinitionName { get; set; }
        public string ViewPermissions { get; set; }
        public string EditPermissions { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public int PageModuleId { get; set; }
        [NotMapped]
        public int PageId { get; set; }
        [NotMapped]
        public string Title { get; set; }
        [NotMapped]
        public string Pane { get; set; }
        [NotMapped]
        public int Order { get; set; }
        [NotMapped]
        public string ContainerType { get; set; }
        [NotMapped]
        public string ModuleType { get; set; }
        [NotMapped]
        public int PaneModuleIndex { get; set; }
        [NotMapped]
        public int PaneModuleCount { get; set; }
    }
}

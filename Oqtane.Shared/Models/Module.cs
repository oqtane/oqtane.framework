using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Module
    {
        public int ModuleId { get; set; }
        public int SiteId { get; set; }
        public string ModuleDefinitionName { get; set; }
        public string ViewPermissions { get; set; }
        public string EditPermissions { get; set; }
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

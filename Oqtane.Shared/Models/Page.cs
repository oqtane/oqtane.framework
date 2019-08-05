using System;

namespace Oqtane.Models
{
    public class Page : IAuditable
    {
        public int PageId { get; set; }
        public int SiteId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
        public string ThemeType { get; set; }
        public string LayoutType { get; set; }
        public string Icon { get; set; }
        public string Panes { get; set; }
        public string ViewPermissions { get; set; }
        public string EditPermissions { get; set; }
        public bool IsNavigation { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

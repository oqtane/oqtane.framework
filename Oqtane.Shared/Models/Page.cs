using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsNavigation { get; set; }
        public bool EditMode { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }


        [NotMapped]
        public string Permissions { get; set; }
    }
}

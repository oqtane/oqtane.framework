using System;

namespace Oqtane.Models
{
    public class Site : IAuditable
    {
        public int SiteId { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string DefaultThemeType { get; set; }


        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

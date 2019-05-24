using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Alias
    {
        public int AliasId { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public int SiteId { get; set; }

        [NotMapped]
        public string Scheme { get; set; }

        [NotMapped]
        public string Url
        {
            get
            {
                return Scheme + "://" + Name;
            }
        }

        [NotMapped]
        public string Path
        {
            get
            {
                if (Name.Contains("/"))
                {
                    return Name.Substring(Name.IndexOf("/") + 1);
                }
                else
                {
                    return "";
                }
            }
        }

    }
}

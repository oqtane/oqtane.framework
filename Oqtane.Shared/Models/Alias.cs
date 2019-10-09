using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Alias : IAuditable
    {
        public int AliasId { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public int SiteId { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }


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
        public string BaseUrl
        {
            get
            {
                string name = Name;
                if (name.Contains("/"))
                {
                    name = name.Substring(0, name.IndexOf("/"));
                }
                return Scheme + "://" + name;
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

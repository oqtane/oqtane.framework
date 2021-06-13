using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// An Alias maps a url like `oqtane.my` or `oqtane.my/products` to a <see cref="Oqtane.Models.Site"/> and <see cref="Oqtane.Models.Tenant"/>
    /// </summary>
    public class Alias : IAuditable
    {
        /// <summary>
        /// The primary ID for internal use. It's also used in API calls to identify the site. 
        /// </summary>
        public int AliasId { get; set; }

        /// <summary>
        /// The Alias Name = URL.
        /// The Name contains the entire path - so it can be `oqtane.me`, `www.oqtane.me` or `oqtane.me/products`
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The Tenant this Alias (and the Site) references.
        /// It's important, as anything related to the Alias must be requested from a database, which is found by the Tenant it's in. 
        /// </summary>
        public int TenantId { get; set; }

        /// <summary>
        /// The Site this Alias references. 
        /// </summary>
        public int SiteId { get; set; }

        /// <inheritdoc />
        public string CreatedBy { get; set; }

        /// <inheritdoc />
        public DateTime CreatedOn { get; set; }
        
        /// <inheritdoc />
        public string ModifiedBy { get; set; }
        
        /// <inheritdoc />
        public DateTime ModifiedOn { get; set; }

        /// <summary>
        /// The path contains the url-part after the first slash.
        /// * If the Name is `oqtane.me` the Path is empty
        /// * if the Name is `oqtane.me/products` the Path is `products`
        /// </summary>
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

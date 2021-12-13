using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a UrlMapping in Oqtane.
    /// </summary>
    public class UrlMapping
    {
        /// <summary>
        /// ID of this UrlMapping.
        /// </summary>
        public int UrlMappingId { get; set; }

        /// <summary>
        ///  Reference to a <see cref="Site"/> 
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// A fully quaified Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// A Url the visitor will be redirected to
        /// </summary>
        public string MappedUrl { get; set; }

        /// <summary>
        /// Number of requests all time for the url
        /// </summary>
        public int Requests { get; set; }

        /// <summary>
        /// Date when the url was first requested for the site
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Date when the url was last requested for the site
        /// </summary>
        public DateTime RequestedOn { get; set; }
        
    }
}

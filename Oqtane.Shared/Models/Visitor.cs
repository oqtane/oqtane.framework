using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Visitor in Oqtane.
    /// </summary>
    public class Visitor
    {
        /// <summary>
        /// ID of this Visitor.
        /// </summary>
        public int VisitorId { get; set; }

        /// <summary>
        ///  Reference to a <see cref="Site"/> 
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        ///  Reference to a <see cref="User"/> if applicable
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Number of times a visitor has visited a site 
        /// </summary>
        public int Visits { get; set; }

        /// <summary>
        /// Last recorded IP Address of visitor
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// Last recorded user agent of visitor
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Last recorded language of visitor
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Last recorded Url of visitor
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Last recorded Referrer of visitor
        /// </summary>
        public string Referrer { get; set; }

        /// <summary>
        /// Date the visitor first visited the site
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Date the visitor last visited the site
        /// </summary>
        public DateTime VisitedOn { get; set; }

        /// <summary>
        /// Direct reference to the <see cref="User"/> object (if applicable)
        /// </summary>
        public User User { get; set; }
    }
}

using System;

namespace Oqtane.Models
{
    /// <summary>
    /// A log entry in the events log.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int LogId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime LogDate { get; set; }

        /// <summary>
        /// Reference to the <see cref="Page"/>
        /// </summary>
        public int? PageId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Module"/>
        /// </summary>
        public int? ModuleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="User"/>
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Url if relevant for this log entry.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Name of the server that created this entry
        /// </summary>
        public string Server { get; set; }
        public string Category { get; set; } // usually the full typename of the 
        public string Feature { get; set; }
        public string Function { get; set; }

        /// <summary>
        /// Log level / severity
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Log Message
        /// </summary>
        public string Message { get; set; }
        public string MessageTemplate { get; set; }

        /// <summary>
        /// Information about raised Exceptions
        /// </summary>
        public string Exception { get; set; }
        public string Properties { get; set; }
    }
}

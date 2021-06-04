using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Log / Journal of <see cref="Job"/>s executed.
    /// </summary>
    public class JobLog
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int JobLogId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Job"/> which was run
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Timestamp when the <see cref="Job"/> started. 
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Timestamp when the <see cref="Job"/> ended. 
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// Success information. 
        /// </summary>
        public bool? Succeeded { get; set; }

        /// <summary>
        /// Additional protocol information that was left after the <see cref="Job"/> ran.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Reference to the Job. 
        /// </summary>
        /// <remarks>
        /// It's not clear if this is always populated. 
        /// </remarks>
        public Job Job { get; set; }
    }
}

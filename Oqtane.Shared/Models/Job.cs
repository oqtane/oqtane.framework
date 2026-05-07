using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Definition of a Job which is run on the server
    /// When Jobs run, they create a <see cref="JobLog"/>
    /// </summary>
    public class Job : ModelBase
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Name for simple identification
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The fully qualified type name of the job
        /// </summary>
        public string JobType { get; set; }

        /// <summary>
        /// Unit used in how often the job should run - expects a character like `m` (minutes), `H` (hours) etc.
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// How often the Job should run - see also <see cref="Frequency"/>
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// When the Job is to be run the first time. See also <see cref="EndDate"/>.
        /// </summary>
        public DateTime? StartDate { get; set; }


        /// <summary>
        /// When the job is to be run the last time. See also <see cref="StartDate"/>.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Determines if the Job is activated / enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// If the Job has ever started running
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// If the Job is executing right now. 
        /// </summary>
        public bool IsExecuting { get; set; }

        /// <summary>
        /// When the Job will run again next time.
        /// </summary>
        public DateTime? NextExecution { get; set; }

        /// <summary>
        /// The number of log entries to retain for this job
        /// </summary>
        public int RetentionHistory { get; set; }

        /// <summary>
        /// The maximum duration in minutes that the job is expected to run. If the job is still running after this time, it will assume the job terminated unexpectedly and it will reset itself automatically.
        /// </summary>
        public int MaximumDuration { get; set; }
    }
}

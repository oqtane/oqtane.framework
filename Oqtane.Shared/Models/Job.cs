using System;

namespace Oqtane.Models
{
    public class Job : IAuditable
    {
        public int JobId { get; set; }
        public string Name { get; set; }
        public string JobType { get; set; }
        public string Frequency { get; set; }
        public int Interval { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsStarted { get; set; }
        public bool IsExecuting { get; set; }
        public DateTime? NextExecution { get; set; }
        public int RetentionHistory { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

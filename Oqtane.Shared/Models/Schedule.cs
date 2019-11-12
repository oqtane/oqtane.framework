using System;

namespace Oqtane.Models
{
    public class Schedule : IAuditable
    {
        public int ScheduleId { get; set; }
        public string Name { get; set; }
        public string JobType { get; set; }
        public int Period { get; set; }
        public string Frequency { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsActive { get; set; }
        public int RetentionHistory { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

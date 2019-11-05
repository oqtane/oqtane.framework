using System;

namespace Oqtane.Models
{
    public class ScheduleLog
    {
        public int ScheduleLogId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public bool? Succeeded { get; set; }
        public string Notes { get; set; }
        public DateTime? NextExecution { get; set; }
    }
}

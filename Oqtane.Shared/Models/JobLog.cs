using System;

namespace Oqtane.Models
{
    public class JobLog
    {
        public int JobLogId { get; set; }
        public int JobId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public bool? Succeeded { get; set; }
        public string Notes { get; set; }

        public Job Job { get; set; }
    }
}

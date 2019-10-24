using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Log
    {
        public int LogId { get; set; }
        public int SiteId { get; set; }
        public DateTime LogDate { get; set; }
        public int? PageId { get; set; }
        public int? ModuleId { get; set; }
        public int? UserId { get; set; }
        public string Url { get; set; }
        public string Server { get; set; }
        public string Category { get; set; } // usually the full typename of the 
        public string Feature { get; set; }
        public string Function { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Exception { get; set; }
        public string Properties { get; set; }
    }
}

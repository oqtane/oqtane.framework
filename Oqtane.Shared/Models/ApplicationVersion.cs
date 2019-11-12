using System;

namespace Oqtane.Models
{
    public class ApplicationVersion
    {
        public int ApplicationVersionId { get; set; }
        public string Version { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

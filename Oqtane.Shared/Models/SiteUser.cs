using System;

namespace Oqtane.Models
{
    public class SiteUser : IAuditable
    {
        public int SiteUserId { get; set; }
        public int SiteId { get; set; }
        public int UserId { get; set; }
        public bool IsAuthorized { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

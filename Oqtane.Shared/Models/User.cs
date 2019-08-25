using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class User : IAuditable
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public bool IsSuperUser { get; set; }
        [NotMapped]
        public int SiteId { get; set; }
        [NotMapped]
        public string Roles { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public bool IsAuthenticated { get; set; }
    }
}

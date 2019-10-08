using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class User : IAuditable, IDeletable
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }

        [NotMapped]
        public int SiteId { get; set; }
        [NotMapped]
        public string Roles { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public bool IsAuthenticated { get; set; }
    }
}

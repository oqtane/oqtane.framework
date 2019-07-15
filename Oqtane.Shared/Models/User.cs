using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Roles { get; set; }
        public bool IsSuperUser { get; set; }

        [NotMapped]
        public string Password { get; set; }
        [NotMapped]
        public bool IsAuthenticated { get; set; }
        [NotMapped]
        public bool IsPersistent { get; set; }
    }
}

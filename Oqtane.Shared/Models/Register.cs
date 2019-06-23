
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class Register
    {
        [Key]
        public string userName { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class RegisterStatus
    {
        [Key]
        public string status { get; set; }
        public bool isSuccessful { get; set; }
        public bool requiresVerification { get; set; }
    }
}
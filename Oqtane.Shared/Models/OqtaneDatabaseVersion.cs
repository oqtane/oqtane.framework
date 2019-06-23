using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class OqtaneDatabaseVersion
    {
        [Key]
        public string VersionNumber { get; set; }
    }
}

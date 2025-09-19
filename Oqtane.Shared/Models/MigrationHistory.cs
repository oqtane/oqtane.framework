using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Models
{
    [Table("__EFMigrationsHistory")]
    [Keyless]
    public class MigrationHistory
    {
        public string MigrationId { get; set; }
        public string ProductVersion { get; set; }
        public DateTime AppliedDate { get; set; }
        public string AppliedVersion { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class ConnectionSetting
    {
        [Key]
        public string DatabaseType { get; set; }
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public bool IntegratedSecurity { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
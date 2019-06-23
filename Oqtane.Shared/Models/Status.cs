using System;
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class Status
    {
        [Key]
        public string StatusMessage { get; set; }
        public bool Success { get; set; }
    }
}
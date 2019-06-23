
using System;
using System.ComponentModel.DataAnnotations;

namespace Oqtane.Models
{
    public class VersionInfo
    {
        [Key]
        public string VersionNumber { get; set; }
        public string ManifestLowestVersion { get; set; }
        public string ManifestHighestVersion { get; set; }
        public string ManifestSuccess { get; set; }
        public string ManifestFailure { get; set; }
        public bool isNewDatabase { get; set; }
        public bool isUpToDate { get; set; }
    }
}
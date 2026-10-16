using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Folder Provider in Oqtane
    /// </summary>
    public class FolderProvider
    {
        /// <summary>
        /// Folder Provider Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Folder Provider Settings Control Type
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// Folder Provider Display Name
        /// </summary>
        public string DisplayName { get; set; }
    }
}

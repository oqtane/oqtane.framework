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
    public class FolderConfig : ModelBase
    {
        /// <summary>
        /// ID to identify the folder config
        /// </summary>
        public int FolderConfigId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Folder Provider Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Folder Provider Type
        /// </summary>
        public string Provider { get; set; }
    }
}

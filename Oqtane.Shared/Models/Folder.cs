using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Folder in Oqtane
    /// </summary>
    public class Folder : ModelBase
    {
        /// <summary>
        /// ID to identify the folder
        /// </summary>
        public int FolderId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Reference to the parent <see cref="Folder"/>, if it has a parent folder.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Folder type - based on FolderTypes
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Folder name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path to the folder
        /// TODO: document from where the path starts
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Sorting order of the folder
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// List of image sizes which can be generated dynamically from uploaded images (ie. 200x200,x200,200x)
        /// </summary>
        public string ImageSizes { get; set; }

        /// <summary>
        /// Maximum folder capacity (in bytes)
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Folder is a dependency of the framework and cannot be modified or removed
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Deprecated - not used
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// TODO: todoc what would this contain?
        /// </summary>
        [NotMapped]
        public List<Permission> PermissionList { get; set; }

        /// <summary>
        /// Folder Depth
        /// TODO: todoc Where does this start, so Depth 0 or 1 is where in the file system?
        /// </summary>
        [NotMapped]
        public int Level { get; set; }

        /// <summary>
        /// Information if this folder has sub-items like more <see cref="Folder"/> or <see cref="File"/> objects
        /// </summary>
        [NotMapped]
        public bool HasChildren { get; set; }

        #region Deprecated Properties

        [Obsolete("The Permissions property is deprecated. Use PermissionList instead", false)]
        [NotMapped]
        [JsonIgnore] // exclude from API payload
        public string Permissions
        {
            get
            {
                return JsonSerializer.Serialize(PermissionList);
            }
            set
            {
                PermissionList = JsonSerializer.Deserialize<List<Permission>>(value);
            }
        }

        #endregion
    }
}

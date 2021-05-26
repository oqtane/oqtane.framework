using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Folder in Oqtane
    /// </summary>
    public class Folder : IAuditable
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
        /// TODO: unclear what this is for
        /// </summary>
        public bool IsSystem { get; set; }

        #region IAuditable Properties

        /// <inheritdoc />
        public string CreatedBy { get; set; }

        /// <inheritdoc />
        public DateTime CreatedOn { get; set; }

        /// <inheritdoc />
        public string ModifiedBy { get; set; }

        /// <inheritdoc />
        public DateTime ModifiedOn { get; set; }

        #endregion

        #region Extended IAuditable Properties, may be moved to an Interface some day so not documented yet

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
        
        #endregion

        /// <summary>
        /// TODO: todoc what would this contain?
        /// </summary>
        [NotMapped]
        public string Permissions { get; set; }

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
    }
}

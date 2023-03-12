using System;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Shared;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a File in Oqtane
    /// </summary>
    public class File : ModelBase
    {
        /// <summary>
        /// ID to identify the file
        /// </summary>
        public int FileId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Folder"/>.
        /// Use this if you need to determine what <see cref="Site"/> the file belongs to. 
        /// </summary>
        public int FolderId { get; set; }

        /// <summary>
        /// Name of the file
        /// todo: with extension or not?
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// File name extension like 'jpg'
        /// * Always lower case
        /// * Without the dot (.)
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// The height of an image (if the file is an image) in pixels.
        /// This is calculated at time of Upload, so if the file is manually replaced, the value will be wrong. 
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// The width of an image (if the file is an image) in pixels.
        /// This is calculated at time of Upload, so if the file is manually replaced, the value will be wrong. 
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// Description of a file
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Deprecated - not used
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// Object reference to the <see cref="Folder"/> object.
        /// Use this if you need to determine what <see cref="Site"/> the file belongs to. 
        /// TODO: not sure if this is always populated, must verify and document
        /// </summary>
        public Folder Folder { get; set; }

        /// <summary>
        /// url for accessing file
        /// </summary>
        [NotMapped]
        public string Url { get; set; }
    }
}

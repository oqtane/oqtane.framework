using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Oqtane.Shared;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a File Blob in Oqtane
    /// </summary>
    public class FileBlob : ModelBase
    {
        /// <summary>
        /// ID to identify the file blob.
        /// </summary>
        public int FileBlobId { get; set; }

        /// <summary>
        /// The file id.
        /// </summary>
        public int FolderId { get; set; }

        public string FileName { get; set; }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        public byte[]? FileStream { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
    }
}

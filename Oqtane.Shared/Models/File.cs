using System;

namespace Oqtane.Models
{
    public class File : IAuditable
    {
        public int FileId { get; set; }
        public int FolderId { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public int Size { get; set; }
        public int ImageHeight { get; set; }
        public int ImageWidth { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        public Folder Folder { get; set; }
    }
}

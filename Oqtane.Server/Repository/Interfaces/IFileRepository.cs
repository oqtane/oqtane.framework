using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IFileRepository
    {
        IEnumerable<File> GetFiles(int FolderId);
        File AddFile(File File);
        File UpdateFile(File File);
        File GetFile(int FileId);
        void DeleteFile(int FileId);
    }
}

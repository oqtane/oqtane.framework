using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IFileRepository
    {
        IEnumerable<File> GetFiles(int folderId);
        File AddFile(File file);
        File UpdateFile(File file);
        File GetFile(int fileId);
        File GetFile(int fileId, bool tracking);
        File GetFile(int siteId, string folderPath, string fileName);
        void DeleteFile(int fileId);
        string GetFilePath(int fileId);
        string GetFilePath(File file);
    }
}

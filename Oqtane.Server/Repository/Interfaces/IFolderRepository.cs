using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IFolderRepository
    {
        IEnumerable<Folder> GetFolders(int siteId);
        Folder AddFolder(Folder folder);
        Folder UpdateFolder(Folder folder);
        Folder GetFolder(int folderId);
        Folder GetFolder(int folderId, bool tracking);
        Folder GetFolder(int siteId, string path);
        void DeleteFolder(int folderId);
        string GetFolderPath(int folderId);
        string GetFolderPath(Folder folder);
    }
}

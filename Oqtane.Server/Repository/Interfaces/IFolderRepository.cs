using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IFolderRepository
    {
        IEnumerable<Folder> GetFolders(int SiteId);
        Folder AddFolder(Folder Folder);
        Folder UpdateFolder(Folder Folder);
        Folder GetFolder(int FolderId);
        Folder GetFolder(int SiteId, string Path);
        void DeleteFolder(int FolderId);
    }
}

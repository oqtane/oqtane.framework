using Oqtane.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFolderService
    {
        Task<List<Folder>> GetFoldersAsync(int SiteId);
        Task<Folder> GetFolderAsync(int FolderId);
        Task<Folder> AddFolderAsync(Folder Folder);
        Task<Folder> UpdateFolderAsync(Folder Folder);
        Task UpdateFolderOrderAsync(int SiteId, int FolderId, int? ParentId);
        Task DeleteFolderAsync(int FolderId);
        Task<Folder> GetFolderAsync(int siteId, [NotNull]string folderPath);
    }
}

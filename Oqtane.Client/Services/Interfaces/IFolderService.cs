using Oqtane.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IFolderService
    {
        Task<List<Folder>> GetFoldersAsync(int siteId);
        Task<Folder> GetFolderAsync(int folderId);
        Task<Folder> AddFolderAsync(Folder folder);
        Task<Folder> UpdateFolderAsync(Folder folder);
        Task UpdateFolderOrderAsync(int siteId, int folderId, int? parentId);
        Task DeleteFolderAsync(int folderId);
        Task<Folder> GetFolderAsync(int siteId, [NotNull]string folderPath);
    }
}

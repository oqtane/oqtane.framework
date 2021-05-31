using Oqtane.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to get / create / modify <see cref="Folder"/> objects.
    /// </summary>
    public interface IFolderService
    {
        /// <summary>
        /// Retrieve root folders of a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Folder>> GetFoldersAsync(int siteId);

        /// <summary>
        /// Retrieve the information of one <see cref="Folder"/>
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        Task<Folder> GetFolderAsync(int folderId);

        /// <summary>
        /// Create one Folder using a <see cref="Folder"/> object. 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task<Folder> AddFolderAsync(Folder folder);

        /// <summary>
        /// Update the information about a <see cref="Folder"/>
        /// Use this to rename the folder etc.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task<Folder> UpdateFolderAsync(Folder folder);

        /// <summary>
        /// Update the internal Folder-Order within the list of Folders.
        /// </summary>
        /// <param name="siteId">Reference to the <see cref="Site"/></param>
        /// <param name="folderId">Reference to a <see cref="Folder"/> for the security check</param>
        /// <param name="parentId">Reference to the Parent <see cref="Folder"/> or null - this Folders children will be re-sorted.</param>
        /// <returns></returns>
        Task UpdateFolderOrderAsync(int siteId, int folderId, int? parentId);

        /// <summary>
        /// Delete a <see cref="Folder"/>
        /// </summary>
        /// <param name="folderId">Reference to a <see cref="Folder"/></param>
        /// <returns></returns>
        Task DeleteFolderAsync(int folderId);

        /// <summary>
        /// Get a <see cref="Folder"/> of a <see cref="Site"/> based on the path.
        /// </summary>
        /// <param name="siteId">Reference to the <see cref="Site"/></param>
        /// <param name="folderPath">Path of the folder
        /// TODO: todoc verify exactly from where the folder path must start
        /// </param>
        /// <returns></returns>
        Task<Folder> GetFolderAsync(int siteId, [NotNull]string folderPath);
    }
}

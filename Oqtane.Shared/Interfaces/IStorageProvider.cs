using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Oqtane.Providers
{
    public interface IStorageProvider
    {
        /// <summary>
        /// The storage provider display name.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// The control type used to manage the storage provider configuration.
        /// </summary>
        string SettingControlType { get; }

        /// <summary>
        /// Initializes the provider with required settings.
        /// </summary>
        /// <param name="storageLocationId">The Storage Location Id.</param>
        void Initialize(int storageLocationId);

        #region File APIs

        Task<bool> FileExistsAsync(Models.Folder folder,  string fileName);

        Task<IList<string>> GetFilesAsync(Models.Folder folder);

        Task<long> GetFileSizeAsync(Models.File file);

        Task<long> GetFileSizeAsync(Models.Folder folder, string fileName);

        Task<Stream> GetFileStreamAsync(Models.File file);

        Task<Stream> GetFileStreamAsync(Models.Folder folder, string fileName);

        Task DeleteFileAsync(Models.File file);

        Task MoveFileAsync(Models.File file, Models.Folder destinationFolder, string fileName);

        Task AddFileAsync(Models.Folder folder, string fileName, Stream fileStream);

        Task CopyFileAsync(Models.File file, Models.Folder destinationFolder);


        #endregion

        #region Folder APIs

        Task<bool> FolderExistsAsync(Models.Folder folder);

        Task CreateFolderAsync(Models.Folder folder);

        Task MoveFolderAsync(Models.Folder sourceFolder, string destinationPath);

        Task DeleteFolderAsync(Models.Folder folder);

        Task<IList<string>> GetSubFoldersAsync(Models.Folder parentFolder, bool recursive);

        #endregion
    }
}

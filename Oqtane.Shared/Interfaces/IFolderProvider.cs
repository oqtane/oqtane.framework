using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Oqtane.Providers
{
    public interface IFolderProvider
    {
        /// <summary>
        /// The unique identifier for the provider
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The user-friendly name displayed in the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// The type name of the provider for configuration purposes.
        /// </summary>
        string SettingType { get; }

        /// <summary>
        /// Gets a value indicating whether private folders are supported by the provider.
        /// </summary>
        bool SupportsPrivateFolders { get; }

        /// <summary>
        /// Initializes the provider with required settings.
        /// </summary>
        /// <param name="settings">Dictionary of configuration values.</param>
        void Initialize(IDictionary<string, string> settings);

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

        Task<IList<string>> GetSubFoldersAsync(Models.Folder parentFolder, string folderType, bool recursive);

        #endregion
    }
}

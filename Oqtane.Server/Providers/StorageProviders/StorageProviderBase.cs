using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Providers.StorageProviders
{
    public abstract class StorageProviderBase : IStorageProvider
    {
        public abstract string DisplayName { get; }

        public virtual string SettingControlType { get; } = null;

        public virtual void Initialize(int folderId)
        {
        }

        public abstract Task AddFileAsync(Folder folder, string fileName, Stream fileStream);

        public abstract Task CopyFileAsync(Models.File file, Folder destinationFolder);

        public abstract Task CreateFolderAsync(Folder folder);

        public abstract Task DeleteFileAsync(Models.File file);

        public abstract Task DeleteFolderAsync(Folder folder);

        public abstract Task<bool> FileExistsAsync(Models.File file);

        public abstract Task<bool> FileExistsAsync(Folder folder, string fileName);

        public abstract Task<bool> FolderExistsAsync(Folder folder);

        public abstract Task<string> GetFilePathAsync(Models.File file);

        public abstract Task<IList<string>> GetFilesAsync(Folder folder);

        public abstract Task<long> GetFileSizeAsync(Models.File file);

        public abstract Task<long> GetFileSizeAsync(Folder folder, string fileName);

        public abstract Task<Stream> GetFileStreamAsync(Models.File file);

        public abstract Task<Stream> GetFileStreamAsync(Folder folder, string fileName);

        public abstract Task<IList<string>> GetSubFoldersAsync(Folder parentFolder, bool recursive);

        public abstract Task MoveFileAsync(Models.File file, Folder destinationFolder, string fileName);

        public abstract Task MoveFolderAsync(Folder sourceFolder, string destinationPath);
    }
}

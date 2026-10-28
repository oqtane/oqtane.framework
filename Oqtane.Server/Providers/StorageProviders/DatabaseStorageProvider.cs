using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Repository;

namespace Oqtane.Providers
{
    public class DatabaseStorageProvider : IStorageProvider
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFileBlobRepository _fileBlobRepository;
        private readonly IStorageProviderFactory _storageProviderFactory;

        public DatabaseStorageProvider(
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            IFileBlobRepository fileBlobRepository,
            IStorageProviderFactory storageProviderFactory)
        {
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _fileBlobRepository = fileBlobRepository;
            _storageProviderFactory = storageProviderFactory;
        }

        public virtual string DisplayName { get; } = "Local Database Storage";

        public virtual string SettingControlType { get; } = null;

        public void Initialize(int storageLocationId)
        {
        }

        public virtual async Task<bool> FileExistsAsync(Models.File file)
        {
            var blob = _fileBlobRepository.GetFileBlob(file.FolderId, file.Name);
            return blob != null;
        }

        public virtual async Task<bool> FileExistsAsync(Models.Folder folder, string fileName)
        {
            var blob = _fileBlobRepository.GetFileBlob(folder.FolderId, fileName);
            return blob != null;
        }

        public virtual async Task<IList<string>> GetFilesAsync(Folder folder)
        {
            return _fileRepository.GetFiles(folder.FolderId)
                .Select(i => i.Name)
                .ToList();
        }

        public virtual async Task<long> GetFileSizeAsync(Models.File file)
        {
            var blob = _fileBlobRepository.GetFileBlob(file.FolderId, file.Name);
            return blob?.Blob?.Length ?? 0;
        }

        public virtual async Task<long> GetFileSizeAsync(Models.Folder folder, string fileName)
        {
            var blob = _fileBlobRepository.GetFileBlob(folder.FolderId, fileName);
            return blob?.Blob?.Length ?? 0;
        }

        public virtual async Task<Stream> GetFileStreamAsync(Models.File file)
        {
            var blob = _fileBlobRepository.GetFileBlob(file.FolderId, file.Name);
            if(blob?.Blob != null)
            {
                return new MemoryStream(blob.Blob);
            }

            return null;
        }

        public virtual async Task<Stream> GetFileStreamAsync(Models.Folder folder, string fileName)
        {
            var blob = _fileBlobRepository.GetFileBlob(folder.FolderId, fileName);
            if (blob?.Blob != null)
            {
                return new MemoryStream(blob.Blob);
            }

            return null;
        }

        public virtual async Task DeleteFileAsync(Models.File file)
        {
            _fileBlobRepository.DeleteFileBlob(file.FolderId, file.Name);
        }

        public virtual async Task MoveFileAsync(Models.File file, Models.Folder destinationFolder, string fileName)
        {
            var sourceStorageProvider = _storageProviderFactory.GetProvider(file.Folder);
            using var stream = await sourceStorageProvider.GetFileStreamAsync(file);
            await AddFileAsync(destinationFolder, fileName, stream);
            stream.Close();

            await sourceStorageProvider.DeleteFileAsync(file);
        }

        public virtual async Task CopyFileAsync(Models.File file, Folder destinationFolder)
        {
            var sourceStorageProvider = _storageProviderFactory.GetProvider(file.Folder);
            using var stream = await sourceStorageProvider.GetFileStreamAsync(file);
            await AddFileAsync(destinationFolder, file.Name, stream);
            stream.Close();
        }

        public virtual async Task AddFileAsync(Models.Folder folder, string fileName, Stream fileStream)
        {
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);

            var blob = _fileBlobRepository.GetFileBlob(folder.FolderId, fileName);
            if (blob != null)
            {
                blob.Blob = memoryStream.ToArray();
                _fileBlobRepository.UpdateFileBlob(blob);
            }
            else
            {
                blob = new FileBlob
                {
                    FolderId = folder.FolderId,
                    FileName = fileName,
                    Blob = memoryStream.ToArray()
                };

                _fileBlobRepository.AddFileBlob(blob);
            }
        }

        public virtual async Task<bool> FolderExistsAsync(Models.Folder folder)
        {
            return true;
        }

        public virtual async Task CreateFolderAsync(Models.Folder folder)
        {
            
        }

        public virtual async Task MoveFolderAsync(Models.Folder sourceFolder, string destinationPath)
        {
        }

        public virtual async Task DeleteFolderAsync(Models.Folder folder)
        {
        }

        public virtual async Task<IList<string>> GetSubFoldersAsync(Models.Folder parentFolder, bool recursive)
        {
            var folders = new List<string>();

            var childFolders = _folderRepository.GetFolders(parentFolder.SiteId).Where(i => i.ParentId == parentFolder.FolderId);
            foreach(var folder in childFolders)
            {
                folders.Add(folder.Path);
                if(recursive)
                {
                    folders.AddRange(await GetSubFoldersAsync(folder, recursive));
                }
            }

            return folders;
        }
    }
}

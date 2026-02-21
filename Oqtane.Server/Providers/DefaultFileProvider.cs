using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Interfaces;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Providers
{
    public class DefaultFileProvider : IFolderProvider
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ITenantManager _tenantManager;
        private readonly ILogManager _logger;

        public DefaultFileProvider(
            IWebHostEnvironment environment,
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            ITenantManager tenantManager,
            ILogManager logger)
        {
            _environment = environment;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _tenantManager = tenantManager;
            _logger = logger;
        }

        public string Name => Constants.DefaultFolderProvider;

        public string DisplayName => "Default File System";

        public string SettingType => null;

        public bool SupportsPrivateFolders => true;

        public void Initialize(IDictionary<string, string> settings)
        {
        }

        public async Task<bool> FileExistsAsync(Models.File file)
        {
            var path = GetFilePath(file);
            return System.IO.File.Exists(path);
        }

        public async Task<bool> FileExistsAsync(Models.Folder folder, string fileName)
        {
            var folderPath = GetFolderPath(folder, folder.Path);
            var filePath = Path.Combine(folderPath, fileName);
            return System.IO.File.Exists(filePath);
        }

        public async Task<IList<string>> GetFilesAsync(Folder folder)
        {
            var folderPath = GetFolderPath(folder, folder.Path);
            var fileNames = new List<string>();
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                for(int i = 0; i < files.Length; i++)
                {
                    fileNames.Add(Path.GetFileName(files[i]));
                }
            }

            return fileNames;
        }

        public async Task<long> GetFileSizeAsync(Models.File file)
        {
            var folder = file.Folder ?? _folderRepository.GetFolder(file.FolderId);
            return await GetFileSizeAsync(folder, file.Name);
        }

        public async Task<long> GetFileSizeAsync(Models.Folder folder, string fileName)
        {
            var folderPath = GetFolderPath(folder, folder.Path);
            var filePath = Path.Combine(folderPath, fileName);
            long size = 0;
            if (System.IO.File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                size = fileInfo.Length;
            }

            return size;
        }

        public async Task<Stream> GetFileStreamAsync(Models.File file)
        {
            var folder = file.Folder ?? _folderRepository.GetFolder(file.FolderId);
            return await GetFileStreamAsync(folder, file.Name);
        }

        public async Task<Stream> GetFileStreamAsync(Models.Folder folder, string fileName)
        {
            var filepath = GetFilePath(folder, fileName);
            if (System.IO.File.Exists(filepath))
            {
                return new FileStream(filepath, FileMode.Open, FileAccess.Read);
            }

            return null;
        }

        public async Task DeleteFileAsync(Models.File file)
        {
            var filepath = GetFilePath(file);
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }
        }

        public async Task MoveFileAsync(Models.File file, Models.Folder destinationFolder, string fileName)
        {
            var sourceFilePath = GetFilePath(file);
            var destinationFolderPath = GetFolderPath(destinationFolder, destinationFolder.Path);
            var destinationFilePath = Path.Combine(destinationFolderPath, fileName);

            // Ensure the destination folder exists
            CreateDirectory(destinationFolderPath);

            if (System.IO.File.Exists(sourceFilePath))
            {
                System.IO.File.Move(sourceFilePath, destinationFilePath);
            }
        }

        public async Task CopyFileAsync(Models.File file, Folder destinationFolder)
        {
            var sourceFilePath = GetFilePath(file);
            var destinationFolderPath = GetFolderPath(destinationFolder, destinationFolder.Path);
            var destinationFilePath = Path.Combine(destinationFolderPath, file.Name);

            // Ensure the destination folder exists
            CreateDirectory(destinationFolderPath);

            if (System.IO.File.Exists(sourceFilePath))
            {
                System.IO.File.Copy(sourceFilePath, destinationFilePath);
            }
        }

        public async Task AddFileAsync(Models.Folder folder, string fileName, Stream fileStream)
        {
            var folderPath = GetFolderPath(folder, folder.Path);
            var filePath = Path.Combine(folderPath, fileName);
            // Ensure the folder exists
            CreateDirectory(folderPath);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            var originalPosition = fileStream.Position;
            try
            {
                fileStream.Position = 0;
                using (var outputFileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.CopyToAsync(outputFileStream);
                }
            }
            finally
            {
                fileStream.Position = originalPosition;
            }
        }

        public async Task<bool> FolderExistsAsync(Models.Folder folder)
        {
            var folderpath = GetFolderPath(folder, folder.Path);
            return Directory.Exists(folderpath);
        }

        public async Task CreateFolderAsync(Models.Folder folder)
        {
            var folderPath = GetFolderPath(folder, folder.Path);
            CreateDirectory(folderPath);
        }

        public async Task MoveFolderAsync(Models.Folder sourceFolder, string destinationPath)
        {
            var sourceFolderPath = GetFolderPath(sourceFolder, sourceFolder.Path);
            var destinationFolderPath = GetFolderPath(sourceFolder, destinationPath);

            if (Directory.Exists(sourceFolderPath) && !Directory.Exists(destinationFolderPath))
            {
                Directory.Move(sourceFolderPath, destinationFolderPath);
            }
        }

        public async Task DeleteFolderAsync(Models.Folder folder)
        {
            var folderPath = GetFolderPath(folder, folder.Path);
            if (Directory.Exists(folderPath))
            {
                // remove all files from disk (including thumbnails, etc...)
                foreach (var filePath in Directory.GetFiles(folderPath))
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(folderPath, true);
            }
        }

        public async Task<IList<string>> GetSubFoldersAsync(Models.Folder parentFolder, string folderType, bool recursive)
        {
            var folderPath = GetFolderPath(parentFolder.SiteId, folderType, parentFolder.Path);
            IList<string> folders = new List<string>();

            if (Directory.Exists(folderPath))
            {
                var directories = Directory.GetDirectories(folderPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                
                foreach(var directory in directories)
                {
                    var relativePath = Path.GetRelativePath(folderPath, directory);
                    if(!string.IsNullOrEmpty(relativePath))
                    {
                        folders.Add(relativePath.Replace("\\", "/"));
                    }
                }
            }

            return folders;
        }

        private string GetFolderPath(Folder folder, string folderPath)
        {
            return GetFolderPath(folder.SiteId, folder.Type, folderPath);
        }

        private string GetFolderPath(int siteId, string folderType, string folderPath)
        {
            string path = "";
            switch (folderType)
            {
                case FolderTypes.Private:
                    path = Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", _tenantManager.GetTenant().TenantId.ToString(), "Sites", siteId.ToString(), folderPath);
                    break;
                case FolderTypes.Public:
                    path = Utilities.PathCombine(_environment.WebRootPath, "Content", "Tenants", _tenantManager.GetTenant().TenantId.ToString(), "Sites", siteId.ToString(), folderPath);
                    break;
            }
            return path;
        }

        private string GetFilePath(Models.File file)
        {
            if (file == null)
            {
                return null;
            }
            var folder = file.Folder ?? _folderRepository.GetFolder(file.FolderId);
            return GetFilePath(folder, file.Name);
        }

        private string GetFilePath(Models.Folder folder, string fileName)
        {
            return Path.Combine(GetFolderPath(folder, folder.Path), fileName);
        }

        private void CreateDirectory(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                var path = folderpath.StartsWith(Path.DirectorySeparatorChar) ? Path.DirectorySeparatorChar.ToString() : string.Empty;
                var separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
                var folders = folderpath.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var folder in folders)
                {
                    path = Utilities.PathCombine(path, folder, Path.DirectorySeparatorChar.ToString());
                    if (!Directory.Exists(path))
                    {
                        try
                        {
                            Directory.CreateDirectory(path);
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "Unable To Create Folder {Folder}", path);
                        }
                    }
                }
            }
        }
    }
}

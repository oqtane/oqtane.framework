using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Providers;
using Oqtane.Repository;
using Oqtane.Shared;
using SixLabors.ImageSharp;

namespace Oqtane.Managers
{
    public interface IFolderManager
    {
        Task SyncFolderAsync(Folder folder, bool recursive, bool includeFiles);
    }
    public class FolderManager : IFolderManager
    {
        private readonly IFolderProviderFactory _folderProviderFactory;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ISettingRepository _settingRepository;

        private static readonly IList<string> _reservedPaths = new List<string> //system reserved folder names, should not been deleted even they are not exists in the folder provider.
        {
            "Public/",
            "Users/"
        };

        public FolderManager(
            IFolderProviderFactory folderProviderFactory,
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            ISettingRepository settingRepository)
        {
            _folderProviderFactory = folderProviderFactory;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _settingRepository = settingRepository;
        }

        public async Task SyncFolderAsync(Folder folder, bool recursive, bool includeFiles)
        {
            if(folder == null)
            {
                throw new ArgumentNullException("The folder doesn't exist");
            }

            var baseItems = GetLocalItems(folder, recursive);
            var remoteItems = await GetRemoteItems(folder, recursive);
            MergeItems(baseItems, remoteItems);

            await ProcessItem(folder, baseItems, includeFiles);
        }
        private IList<MergeItem> GetLocalItems(Folder folder, bool recursive)
        {
            var mergeItems = new List<MergeItem>();
            var folders = _folderRepository.GetFolders(folder.SiteId).Where(i => i.ParentId == folder.FolderId);
            var defaultConfigId = _folderProviderFactory.GetDefaultConfigId(folder.SiteId);
            foreach (var childFolder in folders)
            {
                mergeItems.Add(new MergeItem
                {
                    SiteId = folder.SiteId,
                    LocalExists = true,
                    RemoteExists = _reservedPaths.Any(i => childFolder.Path.StartsWith(i, StringComparison.OrdinalIgnoreCase)),
                    ParentFolderPath = folder.Path,
                    FolderName = childFolder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault(),
                    FolderType = childFolder.Type,
                    FolderConfigId = childFolder.FolderConfigId
                });

                if (recursive)
                {
                    mergeItems.AddRange(GetLocalItems(childFolder, recursive));
                }
            }

            return mergeItems;
        }

        private async Task<IList<MergeItem>> GetRemoteItems(Folder folder, bool recursive)
        {
            var mergeItems = new List<MergeItem>();
            var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
            if (folderProvider != null)
            {
                if (folderProvider.SupportsPrivateFolders)
                {
                    mergeItems.AddRange(await GetRemoteItemsByType(folderProvider, folder, FolderTypes.Private, recursive));
                }

                var publicItems = await GetRemoteItemsByType(folderProvider, folder, FolderTypes.Public, recursive);
                foreach (var item in publicItems)
                {
                    if(!mergeItems.Any(i => i.ParentFolderPath.Equals(item.ParentFolderPath, StringComparison.OrdinalIgnoreCase) && i.FolderName.Equals(item.FolderName, StringComparison.OrdinalIgnoreCase)))
                    {
                        mergeItems.Add(item);
                    }
                }
            }

            if (recursive)
            {
                var folders = _folderRepository.GetFolders(folder.SiteId).Where(i => i.ParentId == folder.FolderId);
                foreach (var childFolder in folders)
                {
                    var childItems = await GetRemoteItems(childFolder, recursive);
                    foreach (var childItem in childItems)
                    {
                        var existItem = mergeItems
                            .FirstOrDefault(i => i.ParentFolderPath.Equals(childItem.ParentFolderPath, StringComparison.OrdinalIgnoreCase) && i.FolderName.Equals(childItem.FolderName, StringComparison.OrdinalIgnoreCase));
                        if (existItem != null)
                        {
                            mergeItems.Remove(existItem);
                        }

                        mergeItems.Add(childItem);
                    }

                }
            }

            return mergeItems;
        }

        private async Task<IList<MergeItem>> GetRemoteItemsByType(IFolderProvider folderProvider, Folder folder, string folderType, bool recursive)
        {
            var mergeItems = new List<MergeItem>();
            var subFolders = (await folderProvider.GetSubFoldersAsync(folder, folderType, recursive)).ToList();

            foreach (var subFolder in subFolders)
            {
                if (string.IsNullOrWhiteSpace(subFolder))
                {
                    continue;
                }

                var pathParts = subFolder.Replace("\\", "/").Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var parentParts = string.Join("/", pathParts.Take(pathParts.Length - 1));
                var parentFolderPath = string.IsNullOrEmpty(parentParts) ? "" : $"{parentParts}/";
                var folderName = pathParts.LastOrDefault() ?? subFolder.Replace("\\", "/").Trim('/');

                mergeItems.Add(new MergeItem
                {
                    SiteId = folder.SiteId,
                    LocalExists = false,
                    RemoteExists = true,
                    ParentFolderPath = $"{folder.Path}{parentFolderPath}",
                    FolderName = folderName,
                    FolderType = folderType,
                    FolderConfigId = folder.FolderConfigId
                });
            }

            return mergeItems;
        }

        private void MergeItems(IList<MergeItem> localItems, IList<MergeItem> remoteItems)
        {
            foreach(var localItem in localItems)
            {
                var remoteItem = remoteItems
                    .FirstOrDefault(i => i.ParentFolderPath.Equals(localItem.ParentFolderPath, StringComparison.OrdinalIgnoreCase) && i.FolderName.Equals(localItem.FolderName, StringComparison.OrdinalIgnoreCase));
                if(remoteItem != null)
                {
                    localItem.RemoteExists = true;
                    remoteItems.Remove(remoteItem);
                }
            }

            foreach(var remoteItem in remoteItems)
            {
                localItems.Add(remoteItem);
            }
        }

        private async Task ProcessItem(Folder rootFolder, IList<MergeItem> mergeItems, bool includeFiles)
        {
            foreach (var mergeItem in mergeItems)
            {
                var parentFolder = _folderRepository.GetFolder(mergeItem.SiteId, mergeItem.ParentFolderPath);
                if (mergeItem.RemoteExists && !mergeItem.LocalExists)
                {
                    // Create local folder
                    if (parentFolder != null)
                    {
                        var order = _folderRepository.GetFolders(mergeItem.SiteId)
                            .Where(f => f.ParentId == parentFolder.FolderId)
                            .Select(f => f.Order)
                            .DefaultIfEmpty(0)
                            .Max();
                        order = order == 0 ? 1 : order + 2;
                        var newFolder = new Folder
                        {
                            SiteId = parentFolder.SiteId,
                            ParentId = parentFolder.FolderId,
                            Name = mergeItem.FolderName,
                            Path = $"{parentFolder.Path}{mergeItem.FolderName}/",
                            Type  = mergeItem.FolderType,
                            Order = order,
                            IsSystem = string.IsNullOrEmpty(parentFolder.Path) ? false : parentFolder.IsSystem,
                            PermissionList = parentFolder.PermissionList.Select(i => new Permission
                            {
                                RoleId = i.RoleId,
                                UserId = i.UserId,
                                IsAuthorized = i.IsAuthorized,
                                PermissionName = i.PermissionName
                            }).ToList(),
                            FolderConfigId = mergeItem.FolderConfigId
                        };

                        _folderRepository.AddFolder(newFolder);
                    }
                }
                else if (!mergeItem.RemoteExists && mergeItem.LocalExists)
                {
                    // Delete local folder
                    if (parentFolder != null)
                    {
                        var folderPath = $"{parentFolder.Path}{mergeItem.FolderName}/";
                        var folder = _folderRepository.GetFolder(mergeItem.SiteId, folderPath);
                        if (folder != null && folder.FolderConfigId == parentFolder.FolderConfigId)
                        {
                            DeleteLocalFolder(folder);
                        }
                    }
                }

                if(includeFiles)
                {
                    await SyncFiles(mergeItem.SiteId, mergeItem.ParentFolderPath, mergeItem.FolderName, mergeItem.FolderConfigId);
                }
            }

            if(includeFiles) //sync the files in the root folder
            {
                var folderParts = rootFolder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                var folderName = folderParts.LastOrDefault() ?? string.Empty;
                var parentFolder = string.Join("/", folderParts.Take(folderParts.Count - 1));
                if(!string.IsNullOrEmpty(parentFolder) && !parentFolder.EndsWith("/"))
                {
                    parentFolder += "/";
                }

                await SyncFiles(rootFolder.SiteId, parentFolder, folderName, rootFolder.FolderConfigId);
            }
        }

        private async Task SyncFiles(int siteId, string parentFolder, string folderName, int folderConfigId)
        {
            var folderPath = string.IsNullOrEmpty(folderName) ? string.Empty : $"{parentFolder}{folderName}/";
            var folder = _folderRepository.GetFolder(siteId, folderPath);
            if(folder != null)
            {
                var folderProvider = _folderProviderFactory.GetProvider(folderConfigId);
                if (folderProvider != null)
                {
                    var _imageFiles = _settingRepository.GetSetting(EntityNames.Site, siteId, "ImageFiles")?.SettingValue;
                    _imageFiles = (string.IsNullOrEmpty(_imageFiles)) ? Constants.ImageFiles : _imageFiles;

                    var remoteFiles = await folderProvider.GetFilesAsync(folder);
                    var localFiles = _fileRepository.GetFiles(folder.FolderId).ToList();
                    var size = 0;
                    if (folder.Capacity != 0)
                    {
                        foreach (var f in localFiles)
                        {
                            size += f.Size;
                        }
                    }

                    // Add missing files
                    foreach (var remoteFile in remoteFiles)
                    {
                        if (!HasValidFileExtension(siteId, remoteFile) || !remoteFile.IsPathOrFileValid())
                        {
                            continue;
                        }

                        if (!localFiles.Any(f => f.Name.Equals(remoteFile, StringComparison.OrdinalIgnoreCase)))
                        {
                            var fileStream = await folderProvider.GetFileStreamAsync(folder, remoteFile);
                            var fileSize = (int)fileStream.Length;
                            if (folder.Capacity == 0 || ((size + fileSize) / 1000000) < folder.Capacity)
                            {
                                var newFile = new File
                                {
                                    FolderId = folder.FolderId,
                                    Name = remoteFile,
                                    Size = fileSize,
                                    Extension = System.IO.Path.GetExtension(remoteFile).ToLower().Replace(".", "")
                                };

                                if (_imageFiles.Split(',').Contains(newFile.Extension))
                                {
                                    try
                                    {
                                        using var image = Image.Load(fileStream);
                                        newFile.ImageHeight = image.Height;
                                        newFile.ImageWidth = image.Width;
                                    }
                                    catch
                                    {
                                        // error opening image file
                                    }
                                }

                                _fileRepository.AddFile(newFile);

                                if (folder.Capacity != 0)
                                {
                                    size += fileSize;
                                }
                            }
                        }
                    }

                    // Delete removed files
                    foreach (var localFile in localFiles)
                    {
                        if (!remoteFiles.Any(f => f.Equals(localFile.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            _fileRepository.DeleteFile(localFile.FileId);
                        }
                    }
                }
            }
        }

        private bool HasValidFileExtension(int siteId, string fileName)
        {
            var _uploadableFiles = _settingRepository.GetSetting(EntityNames.Site, siteId, "UploadableFiles")?.SettingValue;
            _uploadableFiles = (string.IsNullOrEmpty(_uploadableFiles)) ? Constants.UploadableFiles : _uploadableFiles;
            return _uploadableFiles.Split(',').Contains(System.IO.Path.GetExtension(fileName).ToLower().Replace(".", ""));
        }

        private void DeleteLocalFolder(Folder folder)
        {
            //delete files
            var files = _fileRepository.GetFiles(folder.FolderId).ToList();
            foreach (var file in files)
            {
                _fileRepository.DeleteFile(file.FileId);
            }

            //delete child folders
            var childFolders = _folderRepository.GetFolders(folder.SiteId).Where(i => i.ParentId == folder.FolderId).ToList();
            foreach(var childFolder in childFolders)
            {
                DeleteLocalFolder(childFolder);
            }

            //delete the folder
            _folderRepository.DeleteFolder(folder.FolderId);
        }

        class MergeItem
        {
            public int SiteId { get; set; }

            public bool LocalExists { get; set; }

            public bool RemoteExists { get; set; }

            public string ParentFolderPath { get; set; }

            public string FolderName { get; set; }

            public string FolderType { get; set; }

            public int FolderConfigId { get; set; }
        }
    }
}

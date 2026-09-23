using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Interfaces;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Oqtane.Shared;
using System.IO;
using Oqtane.Providers;

namespace Oqtane.Modules.Admin.Files.Manager
{
    public class FileManager : ISearchable
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IFolderProviderFactory _folderProviderFactory;
        private const string DocumentExtensions = ".txt,.htm,.html";

        public FileManager(
            IFolderRepository folderRepository,
            IFileRepository fileRepository,
            IFolderProviderFactory folderProviderFactory)
        {
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;
            _folderProviderFactory = folderProviderFactory; 
        }

        public async Task<List<SearchContent>> GetSearchContentsAsync(PageModule pageModule, DateTime lastIndexedOn)
        {
            var searchContents = new List<SearchContent>();

            var folders = _folderRepository.GetFolders(pageModule.Module.SiteId);
            foreach ( var folder in folders)
            {
                bool changed = false;
                bool removed = false;

                if (folder.ModifiedOn >= lastIndexedOn)
                {
                    changed = true;
                }

                var files = _fileRepository.GetFiles(folder.FolderId);
                foreach (var file in files)
                {
                    if (file.ModifiedOn >= lastIndexedOn || changed)
                    {
                        var path = folder.Path + file.Name;

                        var body = "";
                        var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
                        if (await folderProvider.FileExistsAsync(folder, file.Name))
                        {
                            // only non-binary files can be indexed
                            if (DocumentExtensions.Contains(Path.GetExtension(file.Name)))
                            {
                                // get the contents of the file
                                try
                                {
                                    using var stream = await folderProvider.GetFileStreamAsync(file);
                                    StreamReader reader = new StreamReader(stream);
                                    body = reader.ReadToEnd();
                                }
                                catch
                                {
                                    // could not read the file
                                }
                            }
                        }
                        else
                        {
                            removed = true; // file does not exist on disk
                        }

                        var searchContent = new SearchContent
                        {
                            SiteId = folder.SiteId,
                            EntityName = EntityNames.File,
                            EntityId = file.FileId.ToString(),
                            Title = path,
                            Description = "",
                            Body = body,
                            Url = $"{Constants.FileUrl}{folder.Path}{file.Name}",
                            Permissions = $"{EntityNames.Folder}:{folder.FolderId}",
                            ContentModifiedBy = file.ModifiedBy,
                            ContentModifiedOn = file.ModifiedOn,
                            IsDeleted = (removed)
                        };
                        searchContents.Add(searchContent);
                    }
                }
            }

            return searchContents;
        }
    }
}

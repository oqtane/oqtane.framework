using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Oqtane.Services
{
    public class FolderService : ServiceBase, IFolderService
    {
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public FolderService(HttpClient http, SiteState siteState, NavigationManager navigationManager):base(http)
        {
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string ApiUrl => CreateApiUrl(_siteState.Alias, _navigationManager.Uri, EntityNames.Folder);

        public async Task<List<Folder>> GetFoldersAsync(int siteId)
        {
            List<Folder> folders = await GetJsonAsync<List<Folder>>($"{ApiUrl}?siteid={siteId.ToString()}");
            folders = GetFoldersHierarchy(folders);
            return folders;
        }

        public async Task<Folder> GetFolderAsync(int folderId)
        {
            return await GetJsonAsync<Folder>($"{ApiUrl}/{folderId.ToString()}");
        }

        public async Task<Folder> GetFolderAsync(int siteId, [NotNull] string folderPath)
        {
            if (!(folderPath.EndsWith(System.IO.Path.DirectorySeparatorChar) || folderPath.EndsWith(System.IO.Path.AltDirectorySeparatorChar)))
            {
                folderPath = Utilities.PathCombine(folderPath, "\\");
            }
            
            var path = WebUtility.UrlEncode(folderPath);
            
            return await GetJsonAsync<Folder>($"{ApiUrl}/{siteId}/{path}");
        }

        public async Task<Folder> AddFolderAsync(Folder folder)
        {
            return await PostJsonAsync<Folder>(ApiUrl, folder);
        }

        public async Task<Folder> UpdateFolderAsync(Folder folder)
        {
            return await PutJsonAsync<Folder>($"{ApiUrl}/{folder.FolderId.ToString()}", folder);
        }

        public async Task UpdateFolderOrderAsync(int siteId, int folderId, int? parentId)
        {
            var parent = parentId == null
                ? string.Empty
                : parentId.ToString();
            await PutAsync($"{ApiUrl}/?siteid={siteId.ToString()}&folderid={folderId.ToString()}&parentid={parent}");
        }

        public async Task DeleteFolderAsync(int folderId)
        {
            await DeleteAsync($"{ApiUrl}/{folderId.ToString()}");
        }

        private static List<Folder> GetFoldersHierarchy(List<Folder> folders)
        {
            List<Folder> hierarchy = new List<Folder>();
            Action<List<Folder>, Folder> getPath = null;
            var folders1 = folders;
            getPath = (folderList, folder) =>
            {
                IEnumerable<Folder> children;
                int level;
                if (folder == null)
                {
                    level = -1;
                    children = folders1.Where(item => item.ParentId == null);
                }
                else
                {
                    level = folder.Level;
                    children = folders1.Where(item => item.ParentId == folder.FolderId);
                }

                foreach (Folder child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = folders1.Any(item => item.ParentId == child.FolderId);
                    hierarchy.Add(child);
                    if (getPath != null) getPath(folderList, child);
                }
            };
            folders = folders.OrderBy(item => item.Order).ToList();
            getPath(folders, null);

            // add any non-hierarchical items to the end of the list
            foreach (Folder folder in folders)
            {
                if (hierarchy.Find(item => item.FolderId == folder.FolderId) == null)
                {
                    hierarchy.Add(folder);
                }
            }

            return hierarchy;
        }
    }
}

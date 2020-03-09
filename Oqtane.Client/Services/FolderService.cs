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
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public FolderService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string ApiUrl => CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Folder");

        public async Task<List<Folder>> GetFoldersAsync(int SiteId)
        {
            List<Folder> folders = await _http.GetJsonAsync<List<Folder>>(ApiUrl + "?siteid=" + SiteId.ToString());
            folders = GetFoldersHierarchy(folders);
            return folders;
        }

        public async Task<Folder> GetFolderAsync(int FolderId)
        {
            return await _http.GetJsonAsync<Folder>(ApiUrl + "/" + FolderId.ToString());
        }

        public async Task<Folder> GetFolderAsync(int siteId, [NotNull] string folderPath)
        {
            if (!folderPath.EndsWith("\\")) folderPath += "\\";
            var path = WebUtility.UrlEncode(folderPath);
            return await _http.GetJsonAsync<Folder>($"{ApiUrl}/{siteId}/{path}");
        }

        public async Task<Folder> AddFolderAsync(Folder Folder)
        {
            return await _http.PostJsonAsync<Folder>(ApiUrl, Folder);
        }

        public async Task<Folder> UpdateFolderAsync(Folder Folder)
        {
            return await _http.PutJsonAsync<Folder>(ApiUrl + "/" + Folder.FolderId.ToString(), Folder);
        }

        public async Task UpdateFolderOrderAsync(int SiteId, int FolderId, int? ParentId)
        {
            await _http.PutJsonAsync(
                ApiUrl + "/?siteid=" + SiteId.ToString() + "&folderid=" + FolderId.ToString() + "&parentid=" +
                ((ParentId == null) ? "" : ParentId.ToString()), null);
        }

        public async Task DeleteFolderAsync(int FolderId)
        {
            await _http.DeleteAsync(ApiUrl + "/" + FolderId.ToString());
        }

        private static List<Folder> GetFoldersHierarchy(List<Folder> Folders)
        {
            List<Folder> hierarchy = new List<Folder>();
            Action<List<Folder>, Folder> GetPath = null;
            GetPath = (List<Folder> folders, Folder folder) =>
            {
                IEnumerable<Folder> children;
                int level;
                if (folder == null)
                {
                    level = -1;
                    children = Folders.Where(item => item.ParentId == null);
                }
                else
                {
                    level = folder.Level;
                    children = Folders.Where(item => item.ParentId == folder.FolderId);
                }

                foreach (Folder child in children)
                {
                    child.Level = level + 1;
                    child.HasChildren = Folders.Any(item => item.ParentId == child.FolderId);
                    hierarchy.Add(child);
                    GetPath(folders, child);
                }
            };
            Folders = Folders.OrderBy(item => item.Order).ToList();
            GetPath(Folders, null);

            // add any non-hierarchical items to the end of the list
            foreach (Folder folder in Folders)
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

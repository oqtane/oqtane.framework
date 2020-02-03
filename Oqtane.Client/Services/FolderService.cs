using Oqtane.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;
using System;

namespace Oqtane.Services
{
    public class FolderService : ServiceBase, IFolderService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public FolderService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Folder"); }
        }

        public async Task<List<Folder>> GetFoldersAsync(int SiteId)
        {
            List<Folder> folders = await http.GetJsonAsync<List<Folder>>(apiurl + "?siteid=" + SiteId.ToString());
            folders = GetFoldersHierarchy(folders);
            return folders;
        }

        public async Task<Folder> GetFolderAsync(int FolderId)
        {
            return await http.GetJsonAsync<Folder>(apiurl + "/" + FolderId.ToString());
        }

        public async Task<Folder> AddFolderAsync(Folder Folder)
        {
            return await http.PostJsonAsync<Folder>(apiurl, Folder);
        }

        public async Task<Folder> UpdateFolderAsync(Folder Folder)
        {
            return await http.PutJsonAsync<Folder>(apiurl + "/" + Folder.FolderId.ToString(), Folder);
        }

        public async Task UpdateFolderOrderAsync(int SiteId, int FolderId, int? ParentId)
        {
            await http.PutJsonAsync(apiurl + "/?siteid=" + SiteId.ToString() + "&folderid=" + FolderId.ToString() + "&parentid=" + ((ParentId == null) ? "" : ParentId.ToString()), null);
        }

        public async Task DeleteFolderAsync(int FolderId)
        {
            await http.DeleteAsync(apiurl + "/" + FolderId.ToString());
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
                    child.HasChildren = Folders.Where(item => item.ParentId == child.FolderId).Any();
                    hierarchy.Add(child);
                    GetPath(folders, child);
                }
            };
            Folders = Folders.OrderBy(item => item.Order).ToList();
            GetPath(Folders, null);
            return hierarchy;
        }
    }
}

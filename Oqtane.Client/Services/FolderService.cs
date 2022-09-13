using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Shared;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Oqtane.Documentation;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class FolderService : ServiceBase, IFolderService
    {
        public FolderService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("Folder");

        public async Task<List<Folder>> GetFoldersAsync(int siteId)
        {
            return await GetJsonAsync<List<Folder>>($"{ApiUrl}?siteid={siteId}");
        }

        public async Task<Folder> GetFolderAsync(int folderId)
        {
            return await GetJsonAsync<Folder>($"{ApiUrl}/{folderId}");
        }

        public async Task<Folder> GetFolderAsync(int siteId, [NotNull] string folderPath)
        {
            var path = WebUtility.UrlEncode(folderPath);
            return await GetJsonAsync<Folder>($"{ApiUrl}/{siteId}/{path}");
        }

        public async Task<Folder> AddFolderAsync(Folder folder)
        {
            return await PostJsonAsync<Folder>(ApiUrl, folder);
        }

        public async Task<Folder> UpdateFolderAsync(Folder folder)
        {
            return await PutJsonAsync<Folder>($"{ApiUrl}/{folder.FolderId}", folder);
        }

        public async Task UpdateFolderOrderAsync(int siteId, int folderId, int? parentId)
        {
            var parent = parentId == null
                ? string.Empty
                : parentId.ToString();
            await PutAsync($"{ApiUrl}/?siteid={siteId}&folderid={folderId}&parentid={parent}");
        }

        public async Task DeleteFolderAsync(int folderId)
        {
            await DeleteAsync($"{ApiUrl}/{folderId}");
        }
    }
}

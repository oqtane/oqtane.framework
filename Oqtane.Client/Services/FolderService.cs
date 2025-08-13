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
    /// <summary>
    /// Service to get / create / modify <see cref="Folder"/> objects.
    /// </summary>
    public interface IFolderService
    {
        /// <summary>
        /// Retrieve root folders of a <see cref="Site"/>
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Folder>> GetFoldersAsync(int siteId);

        /// <summary>
        /// Retrieve the information of one <see cref="Folder"/>
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        Task<Folder> GetFolderAsync(int folderId);

        /// <summary>
        /// Create one Folder using a <see cref="Folder"/> object. 
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task<Folder> AddFolderAsync(Folder folder);

        /// <summary>
        /// Update the information about a <see cref="Folder"/>
        /// Use this to rename the folder etc.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        Task<Folder> UpdateFolderAsync(Folder folder);

        /// <summary>
        /// Delete a <see cref="Folder"/>
        /// </summary>
        /// <param name="folderId">Reference to a <see cref="Folder"/></param>
        /// <returns></returns>
        Task DeleteFolderAsync(int folderId);

        /// <summary>
        /// Get a <see cref="Folder"/> of a <see cref="Site"/> based on the path.
        /// </summary>
        /// <param name="siteId">Reference to the <see cref="Site"/></param>
        /// <param name="folderPath">Path of the folder
        /// TODO: todoc verify exactly from where the folder path must start
        /// </param>
        /// <returns></returns>
        Task<Folder> GetFolderAsync(int siteId, [NotNull] string folderPath);
    }

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
            return await GetJsonAsync<Folder>($"{ApiUrl}/path/{siteId}/?path={path}");
        }

        public async Task<Folder> AddFolderAsync(Folder folder)
        {
            return await PostJsonAsync<Folder>(ApiUrl, folder);
        }

        public async Task<Folder> UpdateFolderAsync(Folder folder)
        {
            return await PutJsonAsync<Folder>($"{ApiUrl}/{folder.FolderId}", folder);
        }

        public async Task DeleteFolderAsync(int folderId)
        {
            await DeleteAsync($"{ApiUrl}/{folderId}");
        }
    }
}

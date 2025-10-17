using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retrieve profile categories
    /// </summary>
    public interface IProfileCategoryService
    {

        /// <summary>
        /// Returns a list of profile categories
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<string>> GetProfileCategoriesAsync(int siteId);

        /// <summary>
        /// Updates profile categories
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="categories"></param>
        /// <returns></returns>
        Task UpdateProfileCategoriesAsync(int siteId, List<string> categories);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ProfileCategoryService : ServiceBase, IProfileCategoryService
    {
        public ProfileCategoryService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("ProfileCategory");

        public async Task<List<string>> GetProfileCategoriesAsync(int siteId)
        {
            return await GetJsonAsync<List<string>>($"{Apiurl}?siteid={siteId}");
        }

        public async Task UpdateProfileCategoriesAsync(int siteId, List<string> categories)
        {
            await PutJsonAsync($"{Apiurl}?siteid={siteId}", categories);
        }
    }
}

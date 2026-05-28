using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Linq;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Role"/>s on a <see cref="Site"/>
    /// </summary>
    public interface ISiteGroupService
    {
        /// <summary>
        /// Get all <see cref="SiteGroup"/>s
        /// </summary>
        /// <returns></returns>
        Task<List<SiteGroup>> GetSiteGroupsAsync();

        /// <summary>
        /// Get all <see cref="SiteGroup"/>s
        /// </summary>
        /// <returns></returns>
        Task<List<SiteGroup>> GetSiteGroupsAsync(int primarySiteId);

        /// <summary>
        /// Get one specific <see cref="SiteGroup"/>
        /// </summary>
        /// <param name="siteGroupId">ID-reference of a <see cref="SiteGroup"/></param>
        /// <returns></returns>
        Task<SiteGroup> GetSiteGroupAsync(int siteGroupId);

        /// <summary>
        /// Add / save a new <see cref="SiteGroup"/> to the database.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<SiteGroup> AddSiteGroupAsync(SiteGroup siteGroup);

        /// <summary>
        /// Update a <see cref="SiteGroup"/> in the database.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<SiteGroup> UpdateSiteGroupAsync(SiteGroup siteGroup);

        /// <summary>
        /// Delete a <see cref="SiteGroup"/> in the database.
        /// </summary>
        /// <param name="siteGroupId">ID-reference of a <see cref="SiteGroup"/></param>
        /// <returns></returns>
        Task DeleteSiteGroupAsync(int siteGroupId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteGroupService : ServiceBase, ISiteGroupService
    {
        public SiteGroupService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("SiteGroup");

        public async Task<List<SiteGroup>> GetSiteGroupsAsync()
        {
            return await GetSiteGroupsAsync(-1);
        }

        public async Task<List<SiteGroup>> GetSiteGroupsAsync(int primarySiteId)
        {
            return await GetJsonAsync<List<SiteGroup>>($"{Apiurl}?siteid={primarySiteId}", Enumerable.Empty<SiteGroup>().ToList());
        }

        public async Task<SiteGroup> GetSiteGroupAsync(int siteGroupId)
        {
            return await GetJsonAsync<SiteGroup>($"{Apiurl}/{siteGroupId}");
        }

        public async Task<SiteGroup> AddSiteGroupAsync(SiteGroup siteGroup)
        {
            return await PostJsonAsync<SiteGroup>(Apiurl, siteGroup);
        }

        public async Task<SiteGroup> UpdateSiteGroupAsync(SiteGroup siteGroup)
        {
            return await PutJsonAsync<SiteGroup>($"{Apiurl}/{siteGroup.SiteGroupId}", siteGroup);
        }

        public async Task DeleteSiteGroupAsync(int siteGroupId)
        {
            await DeleteAsync($"{Apiurl}/{siteGroupId}");
        }
    }
}

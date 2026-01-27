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
        Task<List<SiteGroup>> GetSiteGroupsAsync(int siteId, int siteGroupDefinitionId);

        /// <summary>
        /// Get one specific <see cref="SiteGroup"/>
        /// </summary>
        /// <param name="siteSiteGroupDefinitionId">ID-reference of a <see cref="SiteGroup"/></param>
        /// <returns></returns>
        Task<SiteGroup> GetSiteGroupAsync(int siteSiteGroupDefinitionId);

        /// <summary>
        /// Get one specific <see cref="SiteGroup"/>
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <param name="siteGroupDefinitionId">ID-reference of a <see cref="SiteGroupDefinition"/></param>
        /// <returns></returns>
        Task<SiteGroup> GetSiteGroupAsync(int siteId, int siteGroupDefinitionId);

        /// <summary>
        /// Add / save a new <see cref="SiteGroup"/> to the database.
        /// </summary>
        /// <param name="siteGroup"></param>
        /// <returns></returns>
        Task<SiteGroup> AddSiteGroupAsync(SiteGroup siteGroup);

        /// <summary>
        /// Update a <see cref="SiteGroup"/> in the database.
        /// </summary>
        /// <param name="siteGroup"></param>
        /// <returns></returns>
        Task<SiteGroup> UpdateSiteGroupAsync(SiteGroup siteGroup);

        /// <summary>
        /// Delete a <see cref="SiteGroup"/> in the database.
        /// </summary>
        /// <param name="siteSiteGroupDefinitionId">ID-reference of a <see cref="SiteGroup"/></param>
        /// <returns></returns>
        Task DeleteSiteGroupAsync(int siteSiteGroupDefinitionId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteGroupService : ServiceBase, ISiteGroupService
    {
        public SiteGroupService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("SiteGroup");

        public async Task<List<SiteGroup>> GetSiteGroupsAsync(int siteId, int siteGroupDefinitionId)
        {
            return await GetJsonAsync<List<SiteGroup>>($"{Apiurl}?siteid={siteId}&groupid={siteGroupDefinitionId}", Enumerable.Empty<SiteGroup>().ToList());
        }

        public async Task<SiteGroup> GetSiteGroupAsync(int siteSiteGroupDefinitionId)
        {
            return await GetJsonAsync<SiteGroup>($"{Apiurl}/{siteSiteGroupDefinitionId}");
        }

        public async Task<SiteGroup> GetSiteGroupAsync(int siteId, int siteGroupDefinitionId)
        {
            var siteGroups = await GetSiteGroupsAsync(siteId, siteGroupDefinitionId);
            if (siteGroups != null && siteGroups.Count > 0)
            {
                return siteGroups[0];
            }
            else
            {
                return null;
            }
        }

        public async Task<SiteGroup> AddSiteGroupAsync(SiteGroup siteGroup)
        {
            return await PostJsonAsync<SiteGroup>(Apiurl, siteGroup);
        }

        public async Task<SiteGroup> UpdateSiteGroupAsync(SiteGroup siteGroup)
        {
            return await PutJsonAsync<SiteGroup>($"{Apiurl}/{siteGroup.SiteGroupDefinitionId}", siteGroup);
        }

        public async Task DeleteSiteGroupAsync(int siteSiteGroupDefinitionId)
        {
            await DeleteAsync($"{Apiurl}/{siteSiteGroupDefinitionId}");
        }
    }
}

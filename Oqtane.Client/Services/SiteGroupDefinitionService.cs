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
    public interface ISiteGroupDefinitionService
    {
        /// <summary>
        /// Get all <see cref="SiteGroupDefinition"/>s
        /// </summary>
        /// <returns></returns>
        Task<List<SiteGroupDefinition>> GetSiteGroupDefinitionsAsync();

        /// <summary>
        /// Get all <see cref="SiteGroupDefinition"/>s
        /// </summary>
        /// <returns></returns>
        Task<List<SiteGroupDefinition>> GetSiteGroupDefinitionsAsync(int primarySiteId);

        /// <summary>
        /// Get one specific <see cref="SiteGroupDefinition"/>
        /// </summary>
        /// <param name="siteGroupDefinitionId">ID-reference of a <see cref="SiteGroupDefinition"/></param>
        /// <returns></returns>
        Task<SiteGroupDefinition> GetSiteGroupDefinitionAsync(int siteGroupDefinitionId);

        /// <summary>
        /// Add / save a new <see cref="SiteGroupDefinition"/> to the database.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<SiteGroupDefinition> AddSiteGroupDefinitionAsync(SiteGroupDefinition siteGroupDefinition);

        /// <summary>
        /// Update a <see cref="SiteGroupDefinition"/> in the database.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        Task<SiteGroupDefinition> UpdateSiteGroupDefinitionAsync(SiteGroupDefinition siteGroupDefinition);

        /// <summary>
        /// Delete a <see cref="SiteGroupDefinition"/> in the database.
        /// </summary>
        /// <param name="siteGroupDefinitionId">ID-reference of a <see cref="SiteGroupDefinition"/></param>
        /// <returns></returns>
        Task DeleteSiteGroupDefinitionAsync(int siteGroupDefinitionId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteGroupDefinitionService : ServiceBase, ISiteGroupDefinitionService
    {
        public SiteGroupDefinitionService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("SiteGroupDefinition");

        public async Task<List<SiteGroupDefinition>> GetSiteGroupDefinitionsAsync()
        {
            return await GetSiteGroupDefinitionsAsync(-1);
        }

        public async Task<List<SiteGroupDefinition>> GetSiteGroupDefinitionsAsync(int primarySiteId)
        {
            return await GetJsonAsync<List<SiteGroupDefinition>>($"{Apiurl}?siteid={primarySiteId}", Enumerable.Empty<SiteGroupDefinition>().ToList());
        }

        public async Task<SiteGroupDefinition> GetSiteGroupDefinitionAsync(int siteGroupDefinitionId)
        {
            return await GetJsonAsync<SiteGroupDefinition>($"{Apiurl}/{siteGroupDefinitionId}");
        }

        public async Task<SiteGroupDefinition> AddSiteGroupDefinitionAsync(SiteGroupDefinition siteGroupDefinition)
        {
            return await PostJsonAsync<SiteGroupDefinition>(Apiurl, siteGroupDefinition);
        }

        public async Task<SiteGroupDefinition> UpdateSiteGroupDefinitionAsync(SiteGroupDefinition siteGroupDefinition)
        {
            return await PutJsonAsync<SiteGroupDefinition>($"{Apiurl}/{siteGroupDefinition.SiteGroupDefinitionId}", siteGroupDefinition);
        }

        public async Task DeleteSiteGroupDefinitionAsync(int siteGroupDefinitionId)
        {
            await DeleteAsync($"{Apiurl}/{siteGroupDefinitionId}");
        }
    }
}

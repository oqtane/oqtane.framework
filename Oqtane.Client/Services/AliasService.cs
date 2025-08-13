using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve and store <see cref="Alias"/> information.
    /// </summary>
    public interface IAliasService
    {
        /// <summary>
        /// Get all aliases in the system
        /// </summary>
        /// <returns></returns>
        Task<List<Alias>> GetAliasesAsync();

        /// <summary>
        /// Get a single alias
        /// </summary>
        /// <param name="aliasId">The <see cref="Oqtane.Models.Alias"/> ID, not to be confused with a <see cref="Oqtane.Models.Site"/> ID</param>
        /// <returns></returns>
        Task<Alias> GetAliasAsync(int aliasId);

        /// <summary>
        /// Save another <see cref="Oqtane.Models.Alias"/> in the DB. It must already contain all the information incl. <see cref="Oqtane.Models.Tenant"/> it belongs to. 
        /// </summary>
        /// <param name="alias">An <see cref="Oqtane.Models.Alias"/> to add.</param>
        /// <returns></returns>
        Task<Alias> AddAliasAsync(Alias alias);

        /// <summary>
        /// Update an <see cref="Oqtane.Models.Alias"/> in the DB. Make sure the object is correctly filled, as it must update an existing record. 
        /// </summary>
        /// <param name="alias">The <see cref="Oqtane.Models.Alias"/> to update.</param>
        /// <returns></returns>
        Task<Alias> UpdateAliasAsync(Alias alias);

        /// <summary>
        /// Remove an <see cref="Oqtane.Models.Alias"/> from the DB. 
        /// </summary>
        /// <param name="aliasId">The Alias ID, not to be confused with a Site ID.</param>
        /// <returns></returns>
        Task DeleteAliasAsync(int aliasId);
    }

    /// <inheritdoc cref="IAliasService" />
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class AliasService : ServiceBase, IAliasService
    {
        /// <summary>
        /// Constructor - should only be used by Dependency Injection
        /// </summary>
        public AliasService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("Alias");

        /// <inheritdoc />
        public async Task<List<Alias>> GetAliasesAsync()
        {
            List<Alias> aliases = await GetJsonAsync<List<Alias>>(ApiUrl, Enumerable.Empty<Alias>().ToList());
            return aliases.OrderBy(item => item.Name).ToList();
        }

        /// <inheritdoc />
        public async Task<Alias> GetAliasAsync(int aliasId)
        {
            return await GetJsonAsync<Alias>($"{ApiUrl}/{aliasId}");
        }

        /// <inheritdoc />
        public async Task<Alias> AddAliasAsync(Alias alias)
        {
            return await PostJsonAsync<Alias>(ApiUrl, alias);
        }

        /// <inheritdoc />
        public async Task<Alias> UpdateAliasAsync(Alias alias)
        {
            return await PutJsonAsync<Alias>($"{ApiUrl}/{alias.AliasId}", alias);
        }
        /// <inheritdoc />
        public async Task DeleteAliasAsync(int aliasId)
        {
            await DeleteAsync($"{ApiUrl}/{aliasId}");
        }
    }
}

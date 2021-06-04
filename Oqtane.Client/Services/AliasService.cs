using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <inheritdoc cref="IAliasService" />
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class AliasService : ServiceBase, IAliasService
    {

        private readonly SiteState _siteState;

        /// <summary>
        /// Constructor - should only be used by Dependency Injection
        /// </summary>
        public AliasService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string ApiUrl => CreateApiUrl("Alias", _siteState.Alias);

        /// <inheritdoc />
        public async Task<List<Alias>> GetAliasesAsync()
        {
            List<Alias> aliases = await GetJsonAsync<List<Alias>>(ApiUrl);
            return aliases.OrderBy(item => item.Name).ToList();
        }

        /// <inheritdoc />
        public async Task<Alias> GetAliasAsync(int aliasId)
        {
            return await GetJsonAsync<Alias>($"{ApiUrl}/{aliasId}");
        }

        /// <inheritdoc />
        public async Task<Alias> GetAliasAsync(string path, DateTime lastSyncDate)
        {
            // tenant agnostic as SiteState does not exist
            return await GetJsonAsync<Alias>($"{CreateApiUrl("Alias", null)}/name/?path={WebUtility.UrlEncode(path)}&sync={lastSyncDate.ToString("yyyyMMddHHmmssfff")}");
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

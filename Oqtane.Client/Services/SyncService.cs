using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <inheritdoc cref="ISyncService" />
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SyncService : ServiceBase, ISyncService
    {

        private readonly SiteState _siteState;

        /// <summary>
        /// Constructor - should only be used by Dependency Injection
        /// </summary>
        public SyncService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string ApiUrl => CreateApiUrl("Sync", _siteState.Alias);

        /// <inheritdoc />
        public async Task<Sync> GetSyncAsync(DateTime lastSyncDate)
        {
            return await GetJsonAsync<Sync>($"{ApiUrl}/{lastSyncDate.ToString("yyyyMMddHHmmssfff")}");
        }
    }
}

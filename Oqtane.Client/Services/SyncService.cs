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
        public SyncService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string ApiUrl => CreateApiUrl("Sync");

        /// <inheritdoc />
        public async Task<Sync> GetSyncAsync(DateTime lastSyncDate)
        {
            return await GetJsonAsync<Sync>($"{ApiUrl}/{lastSyncDate.ToString("yyyyMMddHHmmssfff")}");
        }
    }
}

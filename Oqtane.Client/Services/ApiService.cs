using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using Oqtane.Models;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ApiService : ServiceBase, IApiService
    {
        public ApiService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Api");

        public async Task<List<Api>> GetApisAsync(int siteId)
        {
            return await GetJsonAsync<List<Api>>($"{Apiurl}?siteid={siteId}");
        }

        public async Task<Api> GetApiAsync(int siteId, string entityName)
        {
            return await GetJsonAsync<Api>($"{Apiurl}/{siteId}/{entityName}");
        }

        public async Task UpdateApiAsync(Api api)
        {
            await PostJsonAsync(Apiurl, api);
        }
    }
}

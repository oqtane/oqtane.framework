using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Net;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class UrlMappingService : ServiceBase, IUrlMappingService
    {
        public UrlMappingService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("UrlMapping");

        public async Task<List<UrlMapping>> GetUrlMappingsAsync(int siteId, bool isMapped)
        {
            List<UrlMapping> urlMappings = await GetJsonAsync<List<UrlMapping>>($"{Apiurl}?siteid={siteId}&ismapped={isMapped}");
            return urlMappings.OrderByDescending(item => item.RequestedOn).ToList();
        }

        public async Task<UrlMapping> GetUrlMappingAsync(int urlMappingId)
        {
            return await GetJsonAsync<UrlMapping>($"{Apiurl}/{urlMappingId}");
        }

        public async Task<UrlMapping> GetUrlMappingAsync(int siteId, string url)
        {
            return await GetJsonAsync<UrlMapping>($"{Apiurl}/url/{siteId}?url={WebUtility.UrlEncode(url)}");
        }

        public async Task<UrlMapping> AddUrlMappingAsync(UrlMapping role)
        {
            return await PostJsonAsync<UrlMapping>(Apiurl, role);
        }

        public async Task<UrlMapping> UpdateUrlMappingAsync(UrlMapping role)
        {
            return await PutJsonAsync<UrlMapping>($"{Apiurl}/{role.UrlMappingId}", role);
        }

        public async Task DeleteUrlMappingAsync(int urlMappingId)
        {
            await DeleteAsync($"{Apiurl}/{urlMappingId}");
        }
    }
}

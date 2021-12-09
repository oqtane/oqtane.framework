using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class UrlMappingService : ServiceBase, IUrlMappingService
    {
        
        private readonly SiteState _siteState;

        public UrlMappingService(HttpClient http, SiteState siteState) : base(http)
        {
            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl("UrlMapping", _siteState.Alias);

        public async Task<List<UrlMapping>> GetUrlMappingsAsync(int siteId, bool isMapped)
        {
            List<UrlMapping> urlMappings = await GetJsonAsync<List<UrlMapping>>($"{Apiurl}?siteid={siteId}&ismapped={isMapped}");
            return urlMappings.OrderByDescending(item => item.RequestedOn).ToList();
        }

        public async Task<UrlMapping> GetUrlMappingAsync(int urlMappingId)
        {
            return await GetJsonAsync<UrlMapping>($"{Apiurl}/{urlMappingId}");
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

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
    /// <summary>
    /// Service to manage <see cref="UrlMapping"/>s on a <see cref="Site"/>
    /// </summary>
    public interface IUrlMappingService
    {
        /// <summary>
        /// Get all <see cref="UrlMapping"/>s of this <see cref="Site"/>.
        ///
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<List<UrlMapping>> GetUrlMappingsAsync(int siteId, bool isMapped);

        /// <summary>
        /// Get one specific <see cref="UrlMapping"/>
        /// </summary>
        /// <param name="urlMappingId">ID-reference of a <see cref="UrlMapping"/></param>
        /// <returns></returns>
        Task<UrlMapping> GetUrlMappingAsync(int urlMappingId);

        /// <summary>
        /// Get one specific <see cref="UrlMapping"/>
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <param name="url">A url</param>
        /// <returns></returns>
        Task<UrlMapping> GetUrlMappingAsync(int siteId, string url);

        /// <summary>
        /// Add / save a new <see cref="UrlMapping"/> to the database.
        /// </summary>
        /// <param name="urlMapping"></param>
        /// <returns></returns>
        Task<UrlMapping> AddUrlMappingAsync(UrlMapping urlMapping);

        /// <summary>
        /// Update a <see cref="UrlMapping"/> in the database.
        /// </summary>
        /// <param name="urlMapping"></param>
        /// <returns></returns>
        Task<UrlMapping> UpdateUrlMappingAsync(UrlMapping urlMapping);

        /// <summary>
        /// Delete a <see cref="UrlMapping"/> in the database.
        /// </summary>
        /// <param name="urlMappingId">ID-reference of a <see cref="UrlMapping"/></param>
        /// <returns></returns>
        Task DeleteUrlMappingAsync(int urlMappingId);
    }

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

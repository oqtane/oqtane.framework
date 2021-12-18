using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
}

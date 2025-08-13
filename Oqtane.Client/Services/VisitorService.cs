using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using System;
using System.Globalization;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Visitor"/>s on a <see cref="Site"/>
    /// </summary>
    public interface IVisitorService
    {
        /// <summary>
        /// Get all <see cref="Visitor"/>s of this <see cref="Site"/>.
        ///
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<List<Visitor>> GetVisitorsAsync(int siteId, DateTime fromDate);

        /// <summary>
        /// Get a specific <see cref="Visitor"/> of this <see cref="Site"/>.
        ///
        /// </summary>
        /// <param name="visitorId">ID-reference of a <see cref="Visitor"/></param>
        /// <returns></returns>
        Task<Visitor> GetVisitorAsync(int visitorId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class VisitorService : ServiceBase, IVisitorService
    {
        public VisitorService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Visitor");

        public async Task<List<Visitor>> GetVisitorsAsync(int siteId, DateTime fromDate)
        {
            List<Visitor> visitors = await GetJsonAsync<List<Visitor>>($"{Apiurl}?siteid={siteId}&fromdate={fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
            return visitors.OrderByDescending(item => item.VisitedOn).ToList();
        }

        public async Task<Visitor> GetVisitorAsync(int visitorId)
        {
            return await GetJsonAsync<Visitor>($"{Apiurl}/{visitorId}");
        }
    }
}

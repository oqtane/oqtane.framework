using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using System;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class VisitorService : ServiceBase, IVisitorService
    {
        public VisitorService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Visitor");

        public async Task<List<Visitor>> GetVisitorsAsync(int siteId, DateTime fromDate)
        {
            List<Visitor> visitors = await GetJsonAsync<List<Visitor>>($"{Apiurl}?siteid={siteId}&fromdate={fromDate.ToString("dd-MMM-yyyy")}");
            return visitors.OrderByDescending(item => item.VisitedOn).ToList();
        }

        public async Task<Visitor> GetVisitorAsync(int visitorId)
        {
            return await GetJsonAsync<Visitor>($"{Apiurl}/{visitorId}");
        }
    }
}

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
        
        private readonly SiteState _siteState;

        public VisitorService(HttpClient http, SiteState siteState) : base(http)
        {
            
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl("Visitor", _siteState.Alias);

        public async Task<List<Visitor>> GetVisitorsAsync(int siteId, DateTime fromDate)
        {
            List<Visitor> visitors = await GetJsonAsync<List<Visitor>>($"{Apiurl}?siteid={siteId}&fromdate={fromDate.ToString("dd-MMM-yyyy")}");
            return visitors.OrderByDescending(item => item.VisitedOn).ToList();
        }
    }
}

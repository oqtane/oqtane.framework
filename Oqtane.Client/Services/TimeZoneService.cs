using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class TimeZoneService : ServiceBase, ITimeZoneService
    {
        public TimeZoneService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("TimeZone");

        public async Task<List<TimeZone>> GetTimeZonesAsync()
        {
            return await GetJsonAsync<List<TimeZone>>($"{Apiurl}");
        }
    }
}

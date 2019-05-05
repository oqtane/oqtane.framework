using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Oqtane.Services
{
    public class TenantService : ServiceBase, ITenantService
    {
        private readonly HttpClient http;
        private readonly string apiurl;

        public TenantService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            apiurl = CreateApiUrl(urihelper.GetAbsoluteUri(), "Tenant");
        }

        public async Task<Tenant> GetTenantAsync()
        {
            return await http.GetJsonAsync<Tenant>(apiurl);
        }
    }
}

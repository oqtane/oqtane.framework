using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Oqtane.Services
{
    public class SystemService : ServiceBase, ISystemService
    {
        public SystemService(HttpClient http) : base(http) { }

        private string Apiurl => CreateApiUrl("System");

        public async Task<Dictionary<string, string>> GetSystemInfoAsync()
        {
            return await GetJsonAsync<Dictionary<string, string>>(Apiurl);
        }
    }
}

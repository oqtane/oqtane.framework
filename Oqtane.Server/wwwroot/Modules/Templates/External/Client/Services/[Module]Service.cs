using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using [Owner].[Module]s.Models;

namespace [Owner].[Module]s.Services
{
    public class [Module]Service : ServiceBase, I[Module]Service, IService
    {
        private readonly SiteState _siteState;

        public [Module]Service(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

         private string Apiurl => CreateApiUrl(_siteState.Alias, "[Module]");

        public async Task<List<[Module]>> Get[Module]sAsync(int ModuleId)
        {
            List<[Module]> [Module]s = await GetJsonAsync<List<[Module]>>(CreateAuthPolicyUrl($"{Apiurl}?moduleid={ModuleId}", ModuleId));
            return [Module]s.OrderBy(item => item.Name).ToList();
        }

        public async Task<[Module]> Get[Module]Async(int [Module]Id, int ModuleId)
        {
            return await GetJsonAsync<[Module]>(CreateAuthPolicyUrl($"{Apiurl}/{[Module]Id}", ModuleId));
        }

        public async Task<[Module]> Add[Module]Async([Module] [Module])
        {
            return await PostJsonAsync<[Module]>(CreateAuthPolicyUrl($"{Apiurl}?entityid={[Module].ModuleId}", [Module].ModuleId), [Module]);
        }

        public async Task<[Module]> Update[Module]Async([Module] [Module])
        {
            return await PutJsonAsync<[Module]>(CreateAuthPolicyUrl($"{Apiurl}/{[Module].[Module]Id}", [Module].ModuleId), [Module]);
        }

        public async Task Delete[Module]Async(int [Module]Id, int ModuleId)
        {
            await DeleteAsync(CreateAuthPolicyUrl($"{Apiurl}/{[Module]Id}", ModuleId));
        }

        private string CreateAuthPolicyUrl(string Url, int ModuleId)
        {
            if (Url.Contains("?"))
            {
                return Url + "&entityid=" + ModuleId.ToString();
            }
            else
            {
                return Url + "?entityid=" + ModuleId.ToString();
            }
        }
    }
}

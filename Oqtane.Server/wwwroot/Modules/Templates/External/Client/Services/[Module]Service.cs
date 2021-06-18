using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using [Owner].[Module].Models;

namespace [Owner].[Module].Services
{
    public class [Module]Service : ServiceBase, I[Module]Service, IService
    {
        public [Module]Service(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("[Module]");

        public async Task<List<Models.[Module]>> Get[Module]sAsync(int ModuleId)
        {
            List<Models.[Module]> [Module]s = await GetJsonAsync<List<Models.[Module]>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", EntityNames.Module, ModuleId));
            return [Module]s.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.[Module]> Get[Module]Async(int [Module]Id, int ModuleId)
        {
            return await GetJsonAsync<Models.[Module]>(CreateAuthorizationPolicyUrl($"{Apiurl}/{[Module]Id}", EntityNames.Module, ModuleId));
        }

        public async Task<Models.[Module]> Add[Module]Async(Models.[Module] [Module])
        {
            return await PostJsonAsync<Models.[Module]>(CreateAuthorizationPolicyUrl($"{Apiurl}", EntityNames.Module, [Module].ModuleId), [Module]);
        }

        public async Task<Models.[Module]> Update[Module]Async(Models.[Module] [Module])
        {
            return await PutJsonAsync<Models.[Module]>(CreateAuthorizationPolicyUrl($"{Apiurl}/{[Module].[Module]Id}", EntityNames.Module, [Module].ModuleId), [Module]);
        }

        public async Task Delete[Module]Async(int [Module]Id, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{[Module]Id}", EntityNames.Module, ModuleId));
        }
    }
}

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
        private readonly SiteState _siteState;

        public [Module]Service(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

         private string Apiurl => CreateApiUrl(_siteState.Alias, "[Module]");

        public async Task<List<Models.[Module]>> Get[Module]sAsync(int ModuleId)
        {
            List<Models.[Module]> [Module]s = await GetJsonAsync<List<Models.[Module]>>(CreateAuthorizationPolicyUrl($"{Apiurl}?moduleid={ModuleId}", ModuleId));
            return [Module]s.OrderBy(item => item.Name).ToList();
        }

        public async Task<Models.[Module]> Get[Module]Async(int [Module]Id, int ModuleId)
        {
            return await GetJsonAsync<Models.[Module]>(CreateAuthorizationPolicyUrl($"{Apiurl}/{[Module]Id}", ModuleId));
        }

        public async Task<Models.[Module]> Add[Module]Async(Models.[Module] [Module])
        {
            return await PostJsonAsync<Models.[Module]>(CreateAuthorizationPolicyUrl($"{Apiurl}", [Module].ModuleId), [Module]);
        }

        public async Task<Models.[Module]> Update[Module]Async(Models.[Module] [Module])
        {
            return await PutJsonAsync<Models.[Module]>(CreateAuthorizationPolicyUrl($"{Apiurl}/{[Module].[Module]Id}", [Module].ModuleId), [Module]);
        }

        public async Task Delete[Module]Async(int [Module]Id, int ModuleId)
        {
            await DeleteAsync(CreateAuthorizationPolicyUrl($"{Apiurl}/{[Module]Id}", ModuleId));
        }
    }
}

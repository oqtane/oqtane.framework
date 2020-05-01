using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using [Owner].[Module]s.Models;

namespace [Owner].[Module]s.Services
{
    public class [Module]Service : ServiceBase, I[Module]Service, IService
    {
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;

        public [Module]Service(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http)
        {
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

         private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "[Module]"); }
        }

        public async Task<List<[Module]>> Get[Module]sAsync(int ModuleId)
        {
            List<[Module]> [Module]s = await GetJsonAsync<List<[Module]>>(Apiurl + "?moduleid=" + ModuleId.ToString());
            return [Module]s.OrderBy(item => item.Name).ToList();
        }

        public async Task<[Module]> Get[Module]Async(int [Module]Id)
        {
            return await GetJsonAsync<[Module]>(Apiurl + "/" + [Module]Id.ToString());
        }

        public async Task<[Module]> Add[Module]Async([Module] [Module])
        {
            return await PostJsonAsync<[Module]>(Apiurl + "?entityid=" + [Module].ModuleId, [Module]);
        }

        public async Task<[Module]> Update[Module]Async([Module] [Module])
        {
            return await PutJsonAsync<[Module]>(Apiurl + "/" + [Module].[Module]Id + "?entityid=" + [Module].ModuleId, [Module]);
        }

        public async Task Delete[Module]Async(int [Module]Id)
        {
            await DeleteAsync(Apiurl + "/" + [Module]Id.ToString());
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Models.[Module]s;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Services.[Module]s
{
    public class [Module]Service : ServiceBase, I[Module]Service, IService
    {
        private readonly HttpClient _http;
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;

        public [Module]Service(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

         private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "[Module]"); }
        }

        public async Task<List<[Module]>> Get[Module]sAsync(int ModuleId)
        {
            List<[Module]> [Module]s = await _http.GetJsonAsync<List<[Module]>>(Apiurl + "?moduleid=" + ModuleId.ToString());
            return [Module]s.OrderBy(item => item.Name).ToList();
        }

        public async Task<[Module]> Get[Module]Async(int [Module]Id)
        {
            return await _http.GetJsonAsync<[Module]>(Apiurl + "/" + [Module]Id.ToString());
        }

        public async Task<[Module]> Add[Module]Async([Module] [Module])
        {
            return await _http.PostJsonAsync<[Module]>(Apiurl + "?entityid=" + [Module].ModuleId, [Module]);
        }

        public async Task<[Module]> Update[Module]Async([Module] [Module])
        {
            return await _http.PutJsonAsync<[Module]>(Apiurl + "/" + [Module].[Module]Id + "?entityid=" + [Module].ModuleId, [Module]);
        }

        public async Task Delete[Module]Async(int [Module]Id)
        {
            await _http.DeleteAsync(Apiurl + "/" + [Module]Id.ToString());
        }
    }
}

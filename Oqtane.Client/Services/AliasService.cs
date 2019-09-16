using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class AliasService : ServiceBase, IAliasService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public AliasService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Alias"); }
        }

        public async Task<List<Alias>> GetAliasesAsync()
        {
            List<Alias> aliases = await http.GetJsonAsync<List<Alias>>(apiurl);
            return aliases.OrderBy(item => item.Name).ToList();
        }

        public async Task<Alias> GetAliasAsync(int AliasId)
        {
            return await http.GetJsonAsync<Alias>(apiurl + "/" + AliasId.ToString());
        }

        public async Task<Alias> AddAliasAsync(Alias alias)
        {
            return await http.PostJsonAsync<Alias>(apiurl, alias);
        }

        public async Task<Alias> UpdateAliasAsync(Alias alias)
        {
            return await http.PutJsonAsync<Alias>(apiurl + "/" + alias.AliasId.ToString(), alias);
        }
        public async Task DeleteAliasAsync(int AliasId)
        {
            await http.DeleteAsync(apiurl + "/" + AliasId.ToString());
        }
    }
}

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
        private readonly IUriHelper urihelper;

        public AliasService(HttpClient http, SiteState sitestate, IUriHelper urihelper)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.urihelper = urihelper;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, urihelper.GetAbsoluteUri(), "Alias"); }
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

        public async Task AddAliasAsync(Alias alias)
        {
            await http.PostJsonAsync(apiurl, alias);
        }

        public async Task UpdateAliasAsync(Alias alias)
        {
            await http.PutJsonAsync(apiurl + "/" + alias.AliasId.ToString(), alias);
        }
        public async Task DeleteAliasAsync(int AliasId)
        {
            await http.DeleteAsync(apiurl + "/" + AliasId.ToString());
        }
    }
}

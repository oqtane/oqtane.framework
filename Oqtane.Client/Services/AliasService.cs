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
        private readonly IUriHelper urihelper;

        public AliasService(HttpClient http, IUriHelper urihelper)
        {
            this.http = http;
            this.urihelper = urihelper;
        }

        private string apiurl
        {
            get { return CreateApiUrl(urihelper.GetAbsoluteUri(), "Alias"); }
        }

        public async Task<List<Alias>> GetAliasesAsync()
        {
            List<Alias> aliases = await http.GetJsonAsync<List<Alias>>(apiurl);
            return aliases.OrderBy(item => item.Name).ToList();
        }

        public async Task<Alias> GetAliasAsync(int AliasId)
        {
            List<Alias> aliases = await http.GetJsonAsync<List<Alias>>(apiurl);
            return aliases.Where(item => item.AliasId == AliasId).FirstOrDefault();
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

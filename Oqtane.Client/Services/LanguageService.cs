using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class LanguageService : ServiceBase, ILanguageService
    {        
        public LanguageService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Language");

        public async Task<List<Language>> GetLanguagesAsync(int siteId)
        {
            return await GetLanguagesAsync(siteId, "");
        }

        public async Task<List<Language>> GetLanguagesAsync(int siteId, string packageName)
        {
            return await GetJsonAsync<List<Language>>($"{Apiurl}?siteid={siteId}&packagename={packageName}");
        }

        public async Task<Language> GetLanguageAsync(int languageId)
        {
            return await GetJsonAsync<Language>($"{Apiurl}/{languageId}");
        }

        public async Task<Language> AddLanguageAsync(Language language)
        {
            return await PostJsonAsync<Language>(Apiurl, language);
        }

        public async Task DeleteLanguageAsync(int languageId)
        {
            await DeleteAsync($"{Apiurl}/{languageId}");
        }
    }
}

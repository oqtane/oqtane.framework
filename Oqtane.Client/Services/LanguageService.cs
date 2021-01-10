using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class LanguageService : ServiceBase, ILanguageService
    {
        
        private readonly SiteState _siteState;

        public LanguageService(HttpClient http, SiteState siteState) : base(http)
        {
            _siteState = siteState;
        }

        private string Apiurl => CreateApiUrl(_siteState.Alias, "Language");

        public async Task<List<Language>> GetLanguagesAsync(int siteId)
        {
            var languages = await GetJsonAsync<List<Language>>($"{Apiurl}?siteid={siteId}");

            return languages?.OrderBy(l => l.Name).ToList() ?? Enumerable.Empty<Language>().ToList();
        }

        public async Task<Language> GetLanguageAsync(int languageId)
            => await GetJsonAsync<Language>($"{Apiurl}/{languageId}");

        public async Task<Language> AddLanguageAsync(Language language)
            => await PostJsonAsync<Language>(Apiurl, language);

        public async Task DeleteLanguageAsync(int languageId)
            => await DeleteAsync($"{Apiurl}/{languageId}");
    }
}

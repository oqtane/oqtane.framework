using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Documentation;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Language"/> entries
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Returns a list of all available languages for the given <see cref="Site"/> 
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Language>> GetLanguagesAsync(int siteId);

        /// <summary>
        /// Returns a list of all available languages for the given <see cref="Site" /> and package
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        Task<List<Language>> GetLanguagesAsync(int siteId, string packageName);

        /// <summary>
        /// Returns the given language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task<Language> GetLanguageAsync(int languageId);

        /// <summary>
        /// Adds the given language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<Language> AddLanguageAsync(Language language);

        /// <summary>
        /// Edits the given language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        Task EditLanguageAsync(Language language);

        /// <summary>
        /// Deletes the given language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task DeleteLanguageAsync(int languageId);
    }

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

        public async Task EditLanguageAsync(Language language)
        {
            await PutJsonAsync<Language>(Apiurl, language);
        }

        public async Task DeleteLanguageAsync(int languageId)
        {
            await DeleteAsync($"{Apiurl}/{languageId}");
        }
    }
}

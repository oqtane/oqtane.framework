using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// Deletes the given language
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task DeleteLanguageAsync(int languageId);
    }
}

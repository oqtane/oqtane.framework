using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface ILanguageService
    {
        Task<List<Language>> GetLanguagesAsync(int siteId);

        Task<Language> GetLanguageAsync(int languageId);

        Task<Language> AddLanguageAsync(Language language);

        Task DeleteLanguageAsync(int languageId);
    }
}

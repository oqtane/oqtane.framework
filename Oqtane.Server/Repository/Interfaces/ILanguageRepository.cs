using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ILanguageRepository
    {
        IEnumerable<Language> GetLanguages(int siteId);

        Language AddLanguage(Language language);

        Language GetLanguage(int languageId);

        void DeleteLanguage(int languageId);
    }
}

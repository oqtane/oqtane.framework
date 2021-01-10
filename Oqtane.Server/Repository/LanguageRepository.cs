using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class LanguageRepository : ILanguageRepository
    {
        private TenantDBContext _db;

        public LanguageRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<Language> GetLanguages(int siteId) => _db.Languages.Where(l => l.SiteId == siteId);

        public Language AddLanguage(Language language)
        {
            _db.Languages.Add(language);
            _db.SaveChanges();

            return language;
        }

        public Language GetLanguage(int languageId) => _db.Languages.Find(languageId);

        public void DeleteLanguage(int languageId)
        {
            var language = _db.Languages.Find(languageId);
            _db.Languages.Remove(language);
            _db.SaveChanges();
        }
    }
}

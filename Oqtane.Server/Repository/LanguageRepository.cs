using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ILanguageRepository
    {
        IEnumerable<Language> GetLanguages(int siteId);
        Language AddLanguage(Language language);
        void UpdateLanguage(Language language);
        Language GetLanguage(int languageId);
        void DeleteLanguage(int languageId);
    }

    public class LanguageRepository : ILanguageRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public LanguageRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IEnumerable<Language> GetLanguages(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Language.Where(l => l.SiteId == siteId).ToList();
        }

        public Language AddLanguage(Language language)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (language.IsDefault)
            {
                // Ensure all other languages are not set to default
                db.Language
                    .Where(l => l.SiteId == language.SiteId)
                    .ToList()
                    .ForEach(l => l.IsDefault = false);
            }
            language.Name = ""; // stored in database but not used (SQLite limitation)
            db.Language.Add(language);
            db.SaveChanges();

            return language;
        }

        public void UpdateLanguage(Language language)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (language.LanguageId != 0)
            {
                db.Entry(language).State = EntityState.Modified;
            }
            if (language.IsDefault)
            {
                // Ensure all other languages are not set to default
                db.Language
                    .Where(l => l.SiteId == language.SiteId &&
                                l.LanguageId != language.LanguageId)
                    .ToList()
                    .ForEach(l => l.IsDefault = false);
            }

            language.Name = ""; // stored in database but not used (SQLite limitation)
            db.SaveChanges();
        }

        public Language GetLanguage(int languageId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Language.Find(languageId);
        }

        public void DeleteLanguage(int languageId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var language = db.Language.Find(languageId);
            db.Language.Remove(language);
            db.SaveChanges();
        }
    }
}

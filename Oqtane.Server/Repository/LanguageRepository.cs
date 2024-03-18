using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly TenantDBContext _queryContext;

        public LanguageRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _queryContext = _dbContextFactory.CreateDbContext();
        }

        public IEnumerable<Language> GetLanguages(int siteId)
        {
            return _queryContext.Language.Where(l => l.SiteId == siteId);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class SearchContentRepository : ISearchContentRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public SearchContentRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<SearchContent>> GetSearchContentListAsync(SearchQuery searchQuery)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchContentList = db.SearchContent.AsNoTracking()
                .Include(i => i.Properties)
                .Where(i => i.SiteId == searchQuery.SiteId && i.IsActive);

            if (searchQuery.EntityNames != null && searchQuery.EntityNames.Any())
            {
                searchContentList = searchContentList.Where(i => searchQuery.EntityNames.Contains(i.EntityName));
            }

            if (searchQuery.BeginModifiedTimeUtc != DateTime.MinValue)
            {
                searchContentList = searchContentList.Where(i => i.ModifiedTime >= searchQuery.BeginModifiedTimeUtc);
            }

            if (searchQuery.EndModifiedTimeUtc != DateTime.MinValue)
            {
                searchContentList = searchContentList.Where(i => i.ModifiedTime <= searchQuery.EndModifiedTimeUtc);
            }

            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                foreach (var property in searchQuery.Properties)
                {
                    searchContentList = searchContentList.Where(i => i.Properties.Any(p => p.Name == property.Key && p.Value == property.Value));
                }
            }

            var filteredContentList = new List<SearchContent>();
            if (!string.IsNullOrEmpty(searchQuery.Keywords))
            {
                foreach (var keyword in SearchUtils.GetKeywordsList(searchQuery.Keywords))
                {
                    filteredContentList.AddRange(await searchContentList.Where(i => i.Words.Any(w => w.WordSource.Word.StartsWith(keyword))).ToListAsync());
                }
            }

            return filteredContentList.DistinctBy(i => i.UniqueKey);
        }

        public SearchContent AddSearchContent(SearchContent searchContent)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.SearchContent.Add(searchContent);

            if(searchContent.Properties != null && searchContent.Properties.Any())
            {
                foreach(var property in searchContent.Properties)
                {
                    property.SearchContentId = searchContent.SearchContentId;
                    context.SearchContentProperty.Add(property);
                }
            }

            context.SaveChanges();

            return searchContent;
        }

        public void DeleteSearchContent(int searchContentId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchContent = db.SearchContent.Find(searchContentId);
            db.SearchContent.Remove(searchContent);
            db.SaveChanges();
        }

        public void DeleteSearchContent(string entityName, int entryId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchContent = db.SearchContent.FirstOrDefault(i => i.EntityName == entityName && i.EntityId == entryId);
            if(searchContent != null)
            {
                db.SearchContent.Remove(searchContent);
                db.SaveChanges();
            }
        }

        public void DeleteSearchContent(string uniqueKey)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchContent = db.SearchContent.FirstOrDefault(i => (i.EntityName + ":" + i.EntityId) == uniqueKey);
            if (searchContent != null)
            {
                db.SearchContent.Remove(searchContent);
                db.SaveChanges();
            }
        }

        public void DeleteAllSearchContent()
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SearchContent.RemoveRange(db.SearchContent);
            db.SaveChanges();
        }

        public SearchContentWordSource GetSearchContentWordSource(string word)
        {
            if(string.IsNullOrEmpty(word))
            {
                return null;
            }

            using var db = _dbContextFactory.CreateDbContext();
            return db.SearchContentWordSource.FirstOrDefault(i => i.Word == word);
        }

        public SearchContentWordSource AddSearchContentWordSource(SearchContentWordSource wordSource)
        {
                using var db = _dbContextFactory.CreateDbContext();

                db.SearchContentWordSource.Add(wordSource);
                db.SaveChanges();

                return wordSource;
        }

        public IEnumerable<SearchContentWords> GetWords(int searchContentId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SearchContentWords
                .Include(i => i.WordSource)
                .Where(i => i.SearchContentId == searchContentId).ToList();
        }

        public SearchContentWords AddSearchContentWords(SearchContentWords word)
        {
                using var db = _dbContextFactory.CreateDbContext();
                
                db.SearchContentWords.Add(word);
                db.SaveChanges();

                return word;
        }

        public SearchContentWords UpdateSearchContentWords(SearchContentWords word)
        {
            using var db = _dbContextFactory.CreateDbContext();

            db.Entry(word).State = EntityState.Modified;
            db.SaveChanges();

            return word;
        }
    }
}

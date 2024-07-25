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

        public async Task<IEnumerable<SearchContent>> GetSearchContentsAsync(SearchQuery searchQuery)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchContents = db.SearchContent.AsNoTracking()
                .Include(i => i.SearchContentProperties)
                .Include(i => i.SearchContentWords)
                .ThenInclude(w => w.SearchWord)
                .Where(i => i.SiteId == searchQuery.SiteId);

            if (!string.IsNullOrEmpty(searchQuery.IncludeEntities))
            {
                searchContents = searchContents.Where(i => searchQuery.IncludeEntities.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(i.EntityName));
            }

            if (!string.IsNullOrEmpty(searchQuery.ExcludeEntities))
            {
                searchContents = searchContents.Where(i => !searchQuery.ExcludeEntities.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(i.EntityName));
            }

            if (searchQuery.FromDate != DateTime.MinValue)
            {
                searchContents = searchContents.Where(i => i.ContentModifiedOn >= searchQuery.FromDate);
            }

            if (searchQuery.ToDate != DateTime.MaxValue)
            {
                searchContents = searchContents.Where(i => i.ContentModifiedOn <= searchQuery.ToDate);
            }

            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                foreach (var property in searchQuery.Properties)
                {
                    searchContents = searchContents.Where(i => i.SearchContentProperties.Any(p => p.Name == property.Key && p.Value == property.Value));
                }
            }

            var filteredContentList = new List<SearchContent>();
            if (!string.IsNullOrEmpty(searchQuery.Keywords))
            {
                foreach (var keyword in SearchUtils.GetKeywords(searchQuery.Keywords))
                {
                    filteredContentList.AddRange(await searchContents.Where(i => i.SearchContentWords.Any(w => w.SearchWord.Word.StartsWith(keyword))).ToListAsync());
                }
            }

            return filteredContentList.DistinctBy(i => i.UniqueKey);
        }

        public SearchContent AddSearchContent(SearchContent searchContent)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.SearchContent.Add(searchContent);

            if(searchContent.SearchContentProperties != null && searchContent.SearchContentProperties.Any())
            {
                foreach(var property in searchContent.SearchContentProperties)
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

        public void DeleteSearchContent(string entityName, string entryId)
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

        public SearchWord GetSearchWord(string word)
        {
            if(string.IsNullOrEmpty(word))
            {
                return null;
            }

            using var db = _dbContextFactory.CreateDbContext();
            return db.SearchWord.FirstOrDefault(i => i.Word == word);
        }

        public SearchWord AddSearchWord(SearchWord searchWord)
        {
                using var db = _dbContextFactory.CreateDbContext();

                db.SearchWord.Add(searchWord);
                db.SaveChanges();

                return searchWord;
        }

        public IEnumerable<SearchContentWord> GetSearchContentWords(int searchContentId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SearchContentWord
                .Include(i => i.SearchWord)
                .Where(i => i.SearchContentId == searchContentId).ToList();
        }

        public SearchContentWord AddSearchContentWord(SearchContentWord searchContentWord)
        {
                using var db = _dbContextFactory.CreateDbContext();
                
                db.SearchContentWord.Add(searchContentWord);
                db.SaveChanges();

                return searchContentWord;
        }

        public SearchContentWord UpdateSearchContentWord(SearchContentWord searchContentWord)
        {
            using var db = _dbContextFactory.CreateDbContext();

            db.Entry(searchContentWord).State = EntityState.Modified;
            db.SaveChanges();

            return searchContentWord;
        }
    }
}

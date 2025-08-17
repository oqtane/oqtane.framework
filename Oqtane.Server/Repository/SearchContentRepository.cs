using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface ISearchContentRepository
    {
        Task<IEnumerable<SearchContent>> GetSearchContentsAsync(SearchQuery searchQuery);
        SearchContent AddSearchContent(SearchContent searchContent);
        void DeleteSearchContent(int searchContentId);
        void DeleteSearchContent(string entityName, string entryId);
        void DeleteSearchContent(string uniqueKey);
        void DeleteAllSearchContent(int siteId);

        SearchWord GetSearchWord(string word);
        SearchWord AddSearchWord(SearchWord searchWord);

        IEnumerable<SearchContentWord> GetSearchContentWords(int searchContentId);
        SearchContentWord AddSearchContentWord(SearchContentWord searchContentWord);
        SearchContentWord UpdateSearchContentWord(SearchContentWord searchContentWord);
    }

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

            var keywords = SearchUtils.GetKeywords(searchQuery.Keywords);

            var searchContents = db.SearchContentWord
                .AsNoTracking()
                .Include(item => item.SearchContent)
                .Include(item => item.SearchWord)
                .Where(item => item.SearchContent.SiteId == searchQuery.SiteId)
                .FilterByItems(keywords, (item, keyword) => item.SearchWord.Word.StartsWith(keyword), true)
                .GroupBy(item => new
                {
                    item.SearchContent.SearchContentId,
                    item.SearchContent.SiteId,
                    item.SearchContent.EntityName,
                    item.SearchContent.EntityId,
                    item.SearchContent.Title,
                    item.SearchContent.Description,
                    item.SearchContent.Body,
                    item.SearchContent.Url,
                    item.SearchContent.Permissions,
                    item.SearchContent.ContentModifiedBy,
                    item.SearchContent.ContentModifiedOn,
                    item.SearchContent.AdditionalContent,
                    item.SearchContent.CreatedOn
                })
                .Select(result => new SearchContent
                {
                    SearchContentId = result.Key.SearchContentId,
                    SiteId = result.Key.SiteId,
                    EntityName = result.Key.EntityName,
                    EntityId = result.Key.EntityId,
                    Title = result.Key.Title,
                    Description = result.Key.Description,
                    Body = result.Key.Body,
                    Url = result.Key.Url,
                    Permissions = result.Key.Permissions,
                    ContentModifiedBy = result.Key.ContentModifiedBy,
                    ContentModifiedOn = result.Key.ContentModifiedOn,
                    AdditionalContent = result.Key.AdditionalContent,
                    CreatedOn = result.Key.CreatedOn,
                    Count = result.Sum(group => group.Count)
                });

            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                searchContents = searchContents.Include(item => item.SearchContentProperties);
            }

            if (!string.IsNullOrEmpty(searchQuery.IncludeEntities))
            {
                searchContents = searchContents.Where(item => searchQuery.IncludeEntities.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(item.EntityName));
            }

            if (!string.IsNullOrEmpty(searchQuery.ExcludeEntities))
            {
                searchContents = searchContents.Where(item => !searchQuery.ExcludeEntities.Split(',', StringSplitOptions.RemoveEmptyEntries).Contains(item.EntityName));
            }

            if (searchQuery.FromDate.Date != DateTime.MinValue.Date)
            {
                searchContents = searchContents.Where(item => item.ContentModifiedOn >= searchQuery.FromDate);
            }

            if (searchQuery.ToDate.Date != DateTime.MaxValue.Date)
            {
                searchContents = searchContents.Where(item => item.ContentModifiedOn <= searchQuery.ToDate);
            }

            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                foreach (var property in searchQuery.Properties)
                {
                    searchContents = searchContents.Where(item => item.SearchContentProperties.Any(p => p.Name == property.Key && p.Value == property.Value));
                }
            }

            return await searchContents.ToListAsync();
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

        public void DeleteAllSearchContent(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // delete in batches of 100 records
            var searchContents = db.SearchContent.Where(item => item.SiteId == siteId).Take(100).ToList();
            while (searchContents.Count > 0)
            {
                db.SearchContent.RemoveRange(searchContents);
                db.SaveChanges();
                searchContents = db.SearchContent.Where(item => item.SiteId == siteId).Take(100).ToList();
            }
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

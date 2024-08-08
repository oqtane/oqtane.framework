using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

            var keywords = SearchUtils.GetKeywords(searchQuery.Keywords);

            // using dynamic SQL for query performance (this could be replaced with linq if the exact query structure can be replicated)
            var parameters = new List<object>();
            parameters.Add(searchQuery.SiteId);

            var query = "SELECT sc.*, Count ";
            query += "FROM ( ";
            query += "SELECT sc.SearchContentId, SUM(Count) AS Count ";
            query += "FROM SearchContent sc ";
            query += "INNER JOIN SearchContentWord scw ON sc.SearchContentId = scw.SearchContentId ";
            query += "INNER JOIN SearchWord sw ON scw.SearchWordId = sw.SearchWordId ";
            query += "WHERE sc.SiteId = {0} ";
            if (keywords.Count > 0)
            {
                query += "AND ( ";
                for (int index = 0; index < keywords.Count; index++)
                {
                    query += (index == 0 ? "" : "OR ") + "Word LIKE {" + parameters.Count + "} ";
                    parameters.Add(keywords[index] + "%");
                }
                query += " ) ";
            }
            query += "GROUP BY sc.SearchContentId ";
            query += ") AS Scores ";
            query += "INNER JOIN SearchContent sc ON sc.SearchContentId = Scores.SearchContentId ";
            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                query += "LEFT JOIN SearchContentProperty scp ON sc.SearchContentId = scp.SearchContentId ";
            }
            query += "WHERE sc.SiteId = {0} ";
            if (!string.IsNullOrEmpty(searchQuery.IncludeEntities))
            {
                query += "AND sc.EntityName IN ( ";
                var entities = searchQuery.IncludeEntities.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < entities.Length; index++)
                {
                    query += (index == 0 ? "" : ", ") + "{" + parameters.Count + "} ";
                    parameters.Add(entities[index]);
                }
                query += " ) ";
            }
            if (!string.IsNullOrEmpty(searchQuery.ExcludeEntities))
            {
                query += "AND sc.EntityName NOT IN ( ";
                var entities = searchQuery.ExcludeEntities.Split(',', StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < entities.Length; index++)
                {
                    query += (index == 0 ? "" : ", ") + "{" + parameters.Count + "} ";
                    parameters.Add(entities[index]);
                }
                query += " ) ";
            }
            if (searchQuery.FromDate.ToString() != DateTime.MinValue.ToString())
            {
                query += "AND sc.ContentModifiedOn >= {" + parameters.Count + "} ";
                parameters.Add(searchQuery.FromDate);
            }
            if (searchQuery.ToDate.ToString() != DateTime.MaxValue.ToString())
            {
                query += "AND sc.ContentModifiedOn <= {" + parameters.Count + "} ";
                parameters.Add(searchQuery.ToDate);
            }
            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                foreach (var property in searchQuery.Properties)
                {
                    query += "AND ( scp.Key = {" + parameters.Count + "} ";
                    parameters.Add(property.Key);
                    query += "AND scp.Value = {" + parameters.Count + "} ) ";
                    parameters.Add(property.Value);
                }
            }

            return await db.SearchContent.FromSql(FormattableStringFactory.Create(query, parameters.ToArray())).ToListAsync();
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

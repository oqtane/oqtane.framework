using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class SearchDocumentRepository : ISearchDocumentRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public SearchDocumentRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<SearchDocument>> GetSearchDocumentsAsync(SearchQuery searchQuery)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var documents = db.SearchDocument.AsNoTracking()
                .Include(i => i.Properties)
                .Include(i => i.Tags)
                .Where(i => i.SiteId == searchQuery.SiteId);

            if (searchQuery.Sources != null && searchQuery.Sources.Any())
            {
                documents = documents.Where(i => searchQuery.Sources.Contains(i.IndexerName));
            }

            if (searchQuery.BeginModifiedTimeUtc != DateTime.MinValue)
            {
                documents = documents.Where(i => i.ModifiedTime >= searchQuery.BeginModifiedTimeUtc);
            }

            if (searchQuery.EndModifiedTimeUtc != DateTime.MinValue)
            {
                documents = documents.Where(i => i.ModifiedTime <= searchQuery.EndModifiedTimeUtc);
            }

            if (searchQuery.Tags != null && searchQuery.Tags.Any())
            {
                foreach (var tag in searchQuery.Tags)
                {
                    documents = documents.Where(i => i.Tags.Any(t => t.Tag == tag));
                }
            }

            if (searchQuery.Properties != null && searchQuery.Properties.Any())
            {
                foreach (var property in searchQuery.Properties)
                {
                    documents = documents.Where(i => i.Properties.Any(p => p.Name == property.Key && p.Value == property.Value));
                }
            }

            var filteredDocuments = new List<SearchDocument>();
            if (!string.IsNullOrEmpty(searchQuery.Keywords))
            {
                foreach (var keyword in SearchUtils.GetKeywordsList(searchQuery.Keywords))
                {
                    filteredDocuments.AddRange(await documents.Where(i => i.Title.Contains(keyword) || i.Description.Contains(keyword) || i.Body.Contains(keyword)).ToListAsync());
                }
            }

            return filteredDocuments.DistinctBy(i => i.UniqueKey);
        }

        public SearchDocument AddSearchDocument(SearchDocument searchDocument)
        {
            using var context = _dbContextFactory.CreateDbContext();
            context.SearchDocument.Add(searchDocument);

            if(searchDocument.Properties != null && searchDocument.Properties.Any())
            {
                foreach(var property in searchDocument.Properties)
                {
                    property.SearchDocumentId = searchDocument.SearchDocumentId;
                    context.SearchDocumentProperty.Add(property);
                }
            }

            if (searchDocument.Tags != null && searchDocument.Tags.Any())
            {
                foreach (var tag in searchDocument.Tags)
                {
                    tag.SearchDocumentId = searchDocument.SearchDocumentId;
                    context.SearchDocumentTag.Add(tag);
                }
            }

            context.SaveChanges();

            return searchDocument;
        }

        public void DeleteSearchDocument(int searchDocumentId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchDocument = db.SearchDocument.Find(searchDocumentId);
            db.SearchDocument.Remove(searchDocument);
            db.SaveChanges();
        }

        public void DeleteSearchDocument(string indexerName, int entryId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchDocument = db.SearchDocument.FirstOrDefault(i => i.IndexerName == indexerName && i.EntryId == entryId);
            if(searchDocument != null)
            {
                db.SearchDocument.Remove(searchDocument);
                db.SaveChanges();
            }
        }

        public void DeleteSearchDocument(string uniqueKey)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var searchDocument = db.SearchDocument.FirstOrDefault(i => (i.IndexerName + ":" + i.EntryId) == uniqueKey);
            if (searchDocument != null)
            {
                db.SearchDocument.Remove(searchDocument);
                db.SaveChanges();
            }
        }

        public void DeleteAllSearchDocuments()
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SearchDocument.RemoveRange(db.SearchDocument);
            db.SaveChanges();
        }
    }
}

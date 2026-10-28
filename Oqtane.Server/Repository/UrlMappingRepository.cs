using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUrlMappingRepository
    {
        IEnumerable<UrlMapping> GetUrlMappings(int siteId, bool isMapped);
        UrlMapping AddUrlMapping(UrlMapping urlMapping);
        UrlMapping UpdateUrlMapping(UrlMapping urlMapping);
        UrlMapping GetUrlMapping(int urlMappingId);
        UrlMapping GetUrlMapping(int urlMappingId, bool tracking);
        UrlMapping GetUrlMapping(int siteId, string url);
        void DeleteUrlMapping(int urlMappingId);
        int DeleteUrlMappings(int siteId, int age);
        UrlMapping GetUrlMapping(int siteId, string url, string referrer);
    }

    public class UrlMappingRepository : IUrlMappingRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly ISiteRepository _sites;
        private readonly ICacheManager _cache;
        private readonly ITenantManager _tenantManager;

        public UrlMappingRepository(IDbContextFactory<TenantDBContext> dbContextFactory, ISiteRepository sites, ICacheManager cache, ITenantManager tenantManager)
        {
            _dbContextFactory = dbContextFactory;
            _sites = sites;
            _cache = cache;
            _tenantManager = tenantManager;
        }

        public IEnumerable<UrlMapping> GetUrlMappings(int siteId, bool isMapped)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (isMapped)
            {
                return db.UrlMapping
                    .Where(item => item.SiteId == siteId && !string.IsNullOrEmpty(item.MappedUrl))
                    .AsNoTracking()
                    .ToList();
            }
            else
            {
                return db.UrlMapping
                    .Where(item => item.SiteId == siteId && string.IsNullOrEmpty(item.MappedUrl))
                    .AsNoTracking()
                    .ToList();
            }
        }

        public UrlMapping AddUrlMapping(UrlMapping urlMapping)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.UrlMapping.Add(urlMapping);
            db.SaveChanges();
            if (!string.IsNullOrEmpty(urlMapping.MappedUrl))
            {
                ClearCache();
            }
            return urlMapping;
        }

        public UrlMapping UpdateUrlMapping(UrlMapping urlMapping)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(urlMapping).State = EntityState.Modified;
            db.SaveChanges();
            ClearCache();
            return urlMapping;
        }

        public UrlMapping GetUrlMapping(int urlMappingId)
        {
            return GetUrlMapping(urlMappingId, false);
        }

        public UrlMapping GetUrlMapping(int urlMappingId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.UrlMapping
                    .Find(urlMappingId);
            }
            else
            {
                return db.UrlMapping
                    .AsNoTracking()
                    .FirstOrDefault(item => item.UrlMappingId == urlMappingId);
            }
        }

        public UrlMapping GetUrlMapping(int siteId, string url)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.UrlMapping
                .AsNoTracking()
                .FirstOrDefault(item => item.SiteId == siteId && item.Url == url);
        }

        public void DeleteUrlMapping(int urlMappingId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            UrlMapping urlMapping = db.UrlMapping.Find(urlMappingId);
            db.UrlMapping.Remove(urlMapping);
            db.SaveChanges();
            if (!string.IsNullOrEmpty(urlMapping.MappedUrl))
            {
                ClearCache();
            }
        }

        public int DeleteUrlMappings(int siteId, int age)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // delete in batches of 100 records
            var count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var urlMappings = db.UrlMapping.Where(item => item.SiteId == siteId && string.IsNullOrEmpty(item.MappedUrl) && item.RequestedOn < purgedate)
                .OrderBy(item => item.RequestedOn).Take(100).ToList();
            while (urlMappings.Count > 0)
            {
                count += urlMappings.Count;
                db.UrlMapping.RemoveRange(urlMappings);
                db.SaveChanges();
                urlMappings = db.UrlMapping.Where(item => item.SiteId == siteId && string.IsNullOrEmpty(item.MappedUrl) && item.RequestedOn < purgedate)
                    .OrderBy(item => item.RequestedOn).Take(100).ToList();
            }
            return count;
        }

        public UrlMapping GetUrlMapping(int siteId, string url, string referrer)
        {
            var alias = _tenantManager.GetAlias();
            var urlMappings = _cache.GetCache(alias, "UrlMappings", entry =>
            {
                return GetUrlMappings(siteId, true); // only mapped urls
            });

            url = (url.StartsWith("/")) ? url.Substring(1) : url;
            url = (url.Length > 750) ? url.Substring(0, 750) : url;

            var urlMapping = urlMappings.FirstOrDefault(item => item.Url == url);

            if (urlMapping == null)
            {
                var site = _sites.GetSite(siteId);
                if (site.CaptureBrokenUrls)
                {
                    urlMapping = new UrlMapping
                    {
                        SiteId = siteId,
                        Url = url,
                        MappedUrl = "",
                        Requests = 1,
                        Referrer = referrer,
                        CreatedOn = DateTime.UtcNow,
                        RequestedOn = DateTime.UtcNow
                    };
                    try
                    {
                        urlMapping = AddUrlMapping(urlMapping);
                    }
                    catch
                    {
                        // ignore duplicate key exception which can be caused by a race condition
                    }
                }
            }
            else
            {
                urlMapping.Requests += 1;
                urlMapping.RequestedOn = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(referrer))
                {
                    urlMapping.Referrer = referrer;
                }
                urlMapping = UpdateUrlMapping(urlMapping);
            }
            return urlMapping;
        }

        private void ClearCache()
        {
            var alias = _tenantManager.GetAlias();
            _cache.RemoveCache(alias, "UrlMappings");
        }
    }
}

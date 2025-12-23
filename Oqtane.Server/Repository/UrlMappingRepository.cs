using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
        UrlMapping GetUrlMapping(int siteId, string url, string referrer);
        void DeleteUrlMapping(int urlMappingId);
        int DeleteUrlMappings(int siteId, int age);
    }

    public class UrlMappingRepository : IUrlMappingRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly ISiteRepository _sites;

        public UrlMappingRepository(IDbContextFactory<TenantDBContext> dbContextFactory, ISiteRepository sites)
        {
            _dbContextFactory = dbContextFactory;
            _sites = sites;
        }

        public IEnumerable<UrlMapping> GetUrlMappings(int siteId, bool isMapped)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (isMapped)
            {
                return db.UrlMapping.Where(item => item.SiteId == siteId && !string.IsNullOrEmpty(item.MappedUrl)).ToList();
            }
            else
            {
                return db.UrlMapping.Where(item => item.SiteId == siteId && string.IsNullOrEmpty(item.MappedUrl)).ToList();
            }
        }

        public UrlMapping AddUrlMapping(UrlMapping urlMapping)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.UrlMapping.Add(urlMapping);
            db.SaveChanges();
            return urlMapping;
        }

        public UrlMapping UpdateUrlMapping(UrlMapping urlMapping)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(urlMapping).State = EntityState.Modified;
            db.SaveChanges();
            return urlMapping;
        }

        public UrlMapping GetUrlMapping(int urlMappingId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return GetUrlMapping(urlMappingId, true);
        }

        public UrlMapping GetUrlMapping(int urlMappingId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.UrlMapping.Find(urlMappingId);
            }
            else
            {
                return db.UrlMapping.AsNoTracking().FirstOrDefault(item => item.UrlMappingId == urlMappingId);
            }
        }

        public UrlMapping GetUrlMapping(int siteId, string url)
        {
            return GetUrlMapping(siteId, url, "");
        }

        public UrlMapping GetUrlMapping(int siteId, string url, string referrer)
        {
            using var db = _dbContextFactory.CreateDbContext();
            url = (url.StartsWith("/")) ? url.Substring(1) : url;
            url = (url.Length > 750) ? url.Substring(0, 750) : url;
            var urlMapping = db.UrlMapping.Where(item => item.SiteId == siteId && item.Url == url).FirstOrDefault();
            if (urlMapping == null)
            {
                var site = _sites.GetSite(siteId);
                if (site.CaptureBrokenUrls)
                {
                    urlMapping = new UrlMapping();
                    urlMapping.SiteId = siteId;
                    urlMapping.Url = url;
                    urlMapping.MappedUrl = "";
                    urlMapping.Requests = 1;
                    urlMapping.Referrer = referrer;
                    urlMapping.CreatedOn = DateTime.UtcNow;
                    urlMapping.RequestedOn = DateTime.UtcNow;
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

        public void DeleteUrlMapping(int urlMappingId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            UrlMapping urlMapping = db.UrlMapping.Find(urlMappingId);
            db.UrlMapping.Remove(urlMapping);
            db.SaveChanges();
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
    }
}

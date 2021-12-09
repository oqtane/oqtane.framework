using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class UrlMappingRepository : IUrlMappingRepository
    {
        private TenantDBContext _db;

        public UrlMappingRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<UrlMapping> GetUrlMappings(int siteId, bool isMapped)
        {
            if (isMapped)
            {
                return _db.UrlMapping.Where(item => item.SiteId == siteId && !string.IsNullOrEmpty(item.MappedUrl)).Take(200);
            }
            else
            {
                return _db.UrlMapping.Where(item => item.SiteId == siteId && string.IsNullOrEmpty(item.MappedUrl)).Take(200);
            }
        }

        public UrlMapping AddUrlMapping(UrlMapping urlMapping)
        {
            _db.UrlMapping.Add(urlMapping);
            _db.SaveChanges();
            return urlMapping;
        }

        public UrlMapping UpdateUrlMapping(UrlMapping urlMapping)
        {
            _db.Entry(urlMapping).State = EntityState.Modified;
            _db.SaveChanges();
            return urlMapping;
        }

        public UrlMapping GetUrlMapping(int urlMappingId)
        {
            return GetUrlMapping(urlMappingId, true);
        }

        public UrlMapping GetUrlMapping(int urlMappingId, bool tracking)
        {
            if (tracking)
            {
                return _db.UrlMapping.Find(urlMappingId);
            }
            else
            {
                return _db.UrlMapping.AsNoTracking().FirstOrDefault(item => item.UrlMappingId == urlMappingId);
            }
        }

        public UrlMapping GetUrlMapping(int siteId, string url)
        {
            return _db.UrlMapping.Where(item => item.SiteId == siteId && item.Url == url).FirstOrDefault();
        }

        public void DeleteUrlMapping(int urlMappingId)
        {
            UrlMapping urlMapping = _db.UrlMapping.Find(urlMappingId);
            _db.UrlMapping.Remove(urlMapping);
            _db.SaveChanges();
        }
    }
}

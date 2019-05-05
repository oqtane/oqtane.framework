using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private TenantContext db;

        public SiteRepository(TenantContext context)
        {
            db = context;
        }
            
        public IEnumerable<Site> GetSites()
        {
            try
            {
                return db.Site.ToList();
            }
            catch
            {
                throw;
            }
        }

        public void AddSite(Site site)
        {
            try
            {
                db.Site.Add(site);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public void UpdateSite(Site site)
        {
            try
            {
                db.Entry(site).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }

        public Site GetSite(int siteId)
        {
            try
            {
                Site site = db.Site.Find(siteId);
                return site;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteSite(int siteId)
        {
            try
            {
                Site site = db.Site.Find(siteId);
                db.Site.Remove(site);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}

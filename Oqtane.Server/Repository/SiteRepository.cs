using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SiteRepository : ISiteRepository
    {
        private TenantDBContext db;

        public SiteRepository(TenantDBContext context)
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

        public Site AddSite(Site Site)
        {
            try
            {
                db.Site.Add(Site);
                db.SaveChanges();
                return Site;
            }
            catch
            {
                throw;
            }
        }

        public Site UpdateSite(Site Site)
        {
            try
            {
                db.Entry(Site).State = EntityState.Modified;
                db.SaveChanges();
                return Site;
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

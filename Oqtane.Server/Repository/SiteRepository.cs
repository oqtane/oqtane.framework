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
            return db.Site;
        }

        public Site AddSite(Site Site)
        {
            db.Site.Add(Site);
            db.SaveChanges();
            return Site;
        }

        public Site UpdateSite(Site Site)
        {
            db.Entry(Site).State = EntityState.Modified;
            db.SaveChanges();
            return Site;
        }

        public Site GetSite(int siteId)
        {
            return db.Site.Find(siteId);
        }

        public void DeleteSite(int siteId)
        {
            Site site = db.Site.Find(siteId);
            db.Site.Remove(site);
            db.SaveChanges();
        }
    }
}

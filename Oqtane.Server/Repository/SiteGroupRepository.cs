using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteGroupRepository
    {
        IEnumerable<SiteGroup> GetSiteGroups();
        SiteGroup AddSiteGroup(SiteGroup siteGroup);
        SiteGroup UpdateSiteGroup(SiteGroup siteGroup);
        SiteGroup GetSiteGroup(int siteGroupId);
        SiteGroup GetSiteGroup(int siteGroupId, bool tracking);
        void DeleteSiteGroup(int siteGroupId);
    }

    public class SiteGroupRepository : ISiteGroupRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public SiteGroupRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
            
        public IEnumerable<SiteGroup> GetSiteGroups()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SiteGroup.ToList();
        }

        public SiteGroup AddSiteGroup(SiteGroup siteGroup)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SiteGroup.Add(siteGroup);
            db.SaveChanges();
            return siteGroup;
        }

        public SiteGroup UpdateSiteGroup(SiteGroup siteGroup)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(siteGroup).State = EntityState.Modified;
            db.SaveChanges();
            return siteGroup;
        }

        public SiteGroup GetSiteGroup(int siteGroupId)
        {
            return GetSiteGroup(siteGroupId, true);
        }

        public SiteGroup GetSiteGroup(int siteGroupId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.SiteGroup.FirstOrDefault(item => item.SiteGroupId == siteGroupId);
            }
            else
            {
                return db.SiteGroup.AsNoTracking().FirstOrDefault(item => item.SiteGroupId == siteGroupId);
            }
        }

        public void DeleteSiteGroup(int siteGroupId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            SiteGroup group = db.SiteGroup.Find(siteGroupId);
            db.SiteGroup.Remove(group);
            db.SaveChanges();
        }
    }
}

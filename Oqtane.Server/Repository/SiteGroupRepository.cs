using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteGroupRepository
    {
        IEnumerable<SiteGroup> GetSiteGroups();
        IEnumerable<SiteGroup> GetSiteGroups(int siteId, int siteGroupDefinitionId);
        SiteGroup AddSiteGroup(SiteGroup siteGroup);
        SiteGroup UpdateSiteGroup(SiteGroup siteGroup);
        SiteGroup GetSiteGroup(int siteSiteGroupDefinitionId);
        SiteGroup GetSiteGroup(int siteSiteGroupDefinitionId, bool tracking);
        void DeleteSiteGroup(int siteSiteGroupDefinitionId);
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
            return GetSiteGroups(-1, -1);
        }

        public IEnumerable<SiteGroup> GetSiteGroups(int siteId, int siteGroupDefinitionId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SiteGroup
                .Where(item => (siteId == -1 || item.SiteId == siteId) && (siteGroupDefinitionId == -1 || item.SiteGroupDefinitionId == siteGroupDefinitionId))
                .Include(item => item.SiteGroupDefinition) // eager load
                .ToList();
        }

        public SiteGroup AddSiteGroup(SiteGroup SiteGroup)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SiteGroup.Add(SiteGroup);
            db.SaveChanges();
            return SiteGroup;
        }

        public SiteGroup UpdateSiteGroup(SiteGroup SiteGroup)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(SiteGroup).State = EntityState.Modified;
            db.SaveChanges();
            return SiteGroup;
        }

        public SiteGroup GetSiteGroup(int SiteGroupDefinitionId)
        {
            return GetSiteGroup(SiteGroupDefinitionId, true);
        }

        public SiteGroup GetSiteGroup(int SiteGroupDefinitionId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.SiteGroup
                    .Include(item => item.SiteGroupDefinition) // eager load
                    .FirstOrDefault(item => item.SiteGroupDefinitionId == SiteGroupDefinitionId);
            }
            else
            {
                return db.SiteGroup.AsNoTracking()
                    .Include(item => item.SiteGroupDefinition) // eager load 
                    .FirstOrDefault(item => item.SiteGroupDefinitionId == SiteGroupDefinitionId);
            }
        }

        public void DeleteSiteGroup(int SiteGroupDefinitionId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            SiteGroup SiteGroup = db.SiteGroup.Find(SiteGroupDefinitionId);
            db.SiteGroup.Remove(SiteGroup);
            db.SaveChanges();
        }
    }
}

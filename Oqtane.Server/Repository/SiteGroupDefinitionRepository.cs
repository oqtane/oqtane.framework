using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteGroupDefinitionRepository
    {
        IEnumerable<SiteGroupDefinition> GetSiteGroupDefinitions();
        SiteGroupDefinition AddSiteGroupDefinition(SiteGroupDefinition siteGroupDefinition);
        SiteGroupDefinition UpdateSiteGroupDefinition(SiteGroupDefinition siteGroupDefinition);
        SiteGroupDefinition GetSiteGroupDefinition(int siteGroupDefinitionId);
        SiteGroupDefinition GetSiteGroupDefinition(int siteGroupDefinitionId, bool tracking);
        void DeleteSiteGroupDefinition(int siteGroupDefinitionId);
    }

    public class SiteGroupDefinitionRepository : ISiteGroupDefinitionRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public SiteGroupDefinitionRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
            
        public IEnumerable<SiteGroupDefinition> GetSiteGroupDefinitions()
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SiteGroupDefinition.ToList();
        }

        public SiteGroupDefinition AddSiteGroupDefinition(SiteGroupDefinition siteGroupDefinition)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SiteGroupDefinition.Add(siteGroupDefinition);
            db.SaveChanges();
            return siteGroupDefinition;
        }

        public SiteGroupDefinition UpdateSiteGroupDefinition(SiteGroupDefinition siteGroupDefinition)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(siteGroupDefinition).State = EntityState.Modified;
            db.SaveChanges();
            return siteGroupDefinition;
        }

        public SiteGroupDefinition GetSiteGroupDefinition(int siteGroupDefinitionId)
        {
            return GetSiteGroupDefinition(siteGroupDefinitionId, true);
        }

        public SiteGroupDefinition GetSiteGroupDefinition(int siteGroupDefinitionId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.SiteGroupDefinition.FirstOrDefault(item => item.SiteGroupDefinitionId == siteGroupDefinitionId);
            }
            else
            {
                return db.SiteGroupDefinition.AsNoTracking().FirstOrDefault(item => item.SiteGroupDefinitionId == siteGroupDefinitionId);
            }
        }

        public void DeleteSiteGroupDefinition(int siteGroupDefinitionId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            SiteGroupDefinition group = db.SiteGroupDefinition.Find(siteGroupDefinitionId);
            db.SiteGroupDefinition.Remove(group);
            db.SaveChanges();
        }
    }
}

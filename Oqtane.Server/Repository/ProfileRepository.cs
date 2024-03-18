using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;
        private readonly TenantDBContext _queryContext;

        public ProfileRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _queryContext = _dbContextFactory.CreateDbContext();
        }
            
        public IEnumerable<Profile> GetProfiles(int siteId)
        {
            return _queryContext.Profile.Where(item => item.SiteId == siteId || item.SiteId == null);
        }

        public Profile AddProfile(Profile profile)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Profile.Add(profile);
            db.SaveChanges();
            return profile;
        }

        public Profile UpdateProfile(Profile profile)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(profile).State = EntityState.Modified;
            db.SaveChanges();
            return profile;
        }

        public Profile GetProfile(int profileId)
        {
            return GetProfile(profileId, true);
        }

        public Profile GetProfile(int profileId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.Profile.Find(profileId);
            }
            else
            {
                return db.Profile.AsNoTracking().FirstOrDefault(item => item.ProfileId == profileId);
            }
        }

        public void DeleteProfile(int profileId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            var profile = db.Profile.Find(profileId);
            db.Profile.Remove(profile);
            db.SaveChanges();
        }
    }
}

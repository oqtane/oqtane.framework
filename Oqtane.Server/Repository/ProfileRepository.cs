using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IProfileRepository
    {
        IEnumerable<Profile> GetProfiles(int siteId);
        Profile AddProfile(Profile profile);
        Profile UpdateProfile(Profile profile);
        Profile GetProfile(int profileId);
        Profile GetProfile(int profileId, bool tracking);
        void DeleteProfile(int profileId);
    }
    public class ProfileRepository : IProfileRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public ProfileRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
            
        public IEnumerable<Profile> GetProfiles(int siteId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Profile.Where(item => item.SiteId == siteId || item.SiteId == null).ToList();
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

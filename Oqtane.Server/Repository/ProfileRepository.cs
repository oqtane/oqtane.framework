using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private TenantDBContext _db;

        public ProfileRepository(TenantDBContext context)
        {
            _db = context;
        }
            
        public IEnumerable<Profile> GetProfiles(int siteId)
        {
            return _db.Profile.Where(item => item.SiteId == siteId || item.SiteId == null);
        }

        public Profile AddProfile(Profile profile)
        {
            _db.Profile.Add(profile);
            _db.SaveChanges();
            return profile;
        }

        public Profile UpdateProfile(Profile profile)
        {
            _db.Entry(profile).State = EntityState.Modified;
            _db.SaveChanges();
            return profile;
        }

        public Profile GetProfile(int profileId)
        {
            return GetProfile(profileId, true);
        }

        public Profile GetProfile(int profileId, bool tracking)
        {
            if (tracking)
            {
                return _db.Profile.Find(profileId);
            }
            else
            {
                return _db.Profile.AsNoTracking().FirstOrDefault(item => item.ProfileId == profileId);
            }
        }

        public void DeleteProfile(int profileId)
        {
            Profile profile = _db.Profile.Find(profileId);
            _db.Profile.Remove(profile);
            _db.SaveChanges();
        }
    }
}

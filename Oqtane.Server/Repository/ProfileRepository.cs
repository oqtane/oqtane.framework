using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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
            
        public IEnumerable<Profile> GetProfiles(int SiteId)
        {
            return _db.Profile.Where(item => item.SiteId == SiteId || item.SiteId == null);
        }

        public Profile AddProfile(Profile Profile)
        {
            _db.Profile.Add(Profile);
            _db.SaveChanges();
            return Profile;
        }

        public Profile UpdateProfile(Profile Profile)
        {
            _db.Entry(Profile).State = EntityState.Modified;
            _db.SaveChanges();
            return Profile;
        }

        public Profile GetProfile(int ProfileId)
        {
            return _db.Profile.Find(ProfileId);
        }

        public void DeleteProfile(int ProfileId)
        {
            Profile Profile = _db.Profile.Find(ProfileId);
            _db.Profile.Remove(Profile);
            _db.SaveChanges();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private TenantDBContext db;

        public ProfileRepository(TenantDBContext context)
        {
            db = context;
        }
            
        public IEnumerable<Profile> GetProfiles()
        {
            return db.Profile;
        }

        public IEnumerable<Profile> GetProfiles(int SiteId)
        {
            return db.Profile.Where(item => item.SiteId == SiteId || item.SiteId == null);
        }

        public Profile AddProfile(Profile Profile)
        {
            db.Profile.Add(Profile);
            db.SaveChanges();
            return Profile;
        }

        public Profile UpdateProfile(Profile Profile)
        {
            db.Entry(Profile).State = EntityState.Modified;
            db.SaveChanges();
            return Profile;
        }

        public Profile GetProfile(int ProfileId)
        {
            return db.Profile.Find(ProfileId);
        }

        public void DeleteProfile(int ProfileId)
        {
            Profile Profile = db.Profile.Find(ProfileId);
            db.Profile.Remove(Profile);
            db.SaveChanges();
        }
    }
}

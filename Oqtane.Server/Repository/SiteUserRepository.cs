using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SiteUserRepository : ISiteUserRepository
    {
        private TenantDBContext db;

        public SiteUserRepository(TenantDBContext context)
        {
            db = context;
        }

        public IEnumerable<SiteUser> GetSiteUsers()
        {
            return db.SiteUser;
        }
        public IEnumerable<SiteUser> GetSiteUsers(int SiteId)
        {
            return db.SiteUser.Where(item => item.SiteId == SiteId)
                .Include(item => item.User); // eager load users
        }

        public SiteUser AddSiteUser(SiteUser SiteUser)
        {
            db.SiteUser.Add(SiteUser);
            db.SaveChanges();
            return SiteUser;
        }

        public SiteUser UpdateSiteUser(SiteUser SiteUser)
        {
            db.Entry(SiteUser).State = EntityState.Modified;
            db.SaveChanges();
            return SiteUser;
        }

        public SiteUser GetSiteUser(int SiteUserId)
        {
            return db.SiteUser.Include(item => item.User) // eager load users
                .SingleOrDefault(item => item.SiteUserId == SiteUserId);
        }

        public SiteUser GetSiteUser(int SiteId, int UserId)
        {
            return db.SiteUser.Where(item => item.SiteId == SiteId)
                .Where(item => item.UserId == UserId).FirstOrDefault();
        }

        public void DeleteSiteUser(int SiteUserId)
        {
            SiteUser SiteUser = db.SiteUser.Find(SiteUserId);
            db.SiteUser.Remove(SiteUser);
            db.SaveChanges();
        }
    }
}

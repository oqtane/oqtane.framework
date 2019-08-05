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
            try
            {
                return db.SiteUser.ToList();
            }
            catch
            {
                throw;
            }
        }
        public IEnumerable<SiteUser> GetSiteUsers(int SiteId, int UserId)
        {
            try
            {
                List<SiteUser> siteusers = db.SiteUser.Where(item => item.SiteId == SiteId).ToList();
                if (UserId != -1)
                {
                    siteusers = siteusers.Where(item => item.UserId == UserId).ToList();
                }
                return siteusers;
            }
            catch
            {
                throw;
            }
        }

        public SiteUser AddSiteUser(SiteUser SiteUser)
        {
            try
            {
                db.SiteUser.Add(SiteUser);
                db.SaveChanges();
                return SiteUser;
            }
            catch
            {
                throw;
            }
        }

        public SiteUser UpdateSiteUser(SiteUser SiteUser)
        {
            try
            {
                db.Entry(SiteUser).State = EntityState.Modified;
                db.SaveChanges();
                return SiteUser;
            }
            catch
            {
                throw;
            }
        }

        public SiteUser GetSiteUser(int SiteUserId)
        {
            try
            {
                SiteUser SiteUser = db.SiteUser.Find(SiteUserId);
                return SiteUser;
            }
            catch
            {
                throw;
            }
        }

        public void DeleteSiteUser(int SiteUserId)
        {
            try
            {
                SiteUser SiteUser = db.SiteUser.Find(SiteUserId);
                db.SiteUser.Remove(SiteUser);
                db.SaveChanges();
            }
            catch
            {
                throw;
            }
        }
    }
}

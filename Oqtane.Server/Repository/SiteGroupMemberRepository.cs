using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteGroupMemberRepository
    {
        IEnumerable<SiteGroupMember> GetSiteGroupMembers();
        IEnumerable<SiteGroupMember> GetSiteGroupMembers(int siteId, int siteGroupId);
        SiteGroupMember AddSiteGroupMember(SiteGroupMember siteGroupMember);
        SiteGroupMember UpdateSiteGroupMember(SiteGroupMember siteGroupMember);
        SiteGroupMember GetSiteGroupMember(int siteGroupMemberId);
        SiteGroupMember GetSiteGroupMember(int siteGroupMemberId, bool tracking);
        void DeleteSiteGroupMember(int siteGroupMemberId);
    }

    public class SiteGroupMemberRepository : ISiteGroupMemberRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public SiteGroupMemberRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IEnumerable<SiteGroupMember> GetSiteGroupMembers()
        {
            return GetSiteGroupMembers(-1, -1);
        }

        public IEnumerable<SiteGroupMember> GetSiteGroupMembers(int siteId, int siteGroupId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.SiteGroupMember
                .Where(item => (siteId == -1 || item.SiteId == siteId) && (siteGroupId == -1 || item.SiteGroupId == siteGroupId))
                .Include(item => item.SiteGroup) // eager load
                .ToList();
        }

        public SiteGroupMember AddSiteGroupMember(SiteGroupMember siteGroupMember)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.SiteGroupMember.Add(siteGroupMember);
            db.SaveChanges();
            return siteGroupMember;
        }

        public SiteGroupMember UpdateSiteGroupMember(SiteGroupMember siteGroupMember)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Entry(siteGroupMember).State = EntityState.Modified;
            db.SaveChanges();
            return siteGroupMember;
        }

        public SiteGroupMember GetSiteGroupMember(int siteGroupMemberId)
        {
            return GetSiteGroupMember(siteGroupMemberId, true);
        }

        public SiteGroupMember GetSiteGroupMember(int siteGroupMemberId, bool tracking)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (tracking)
            {
                return db.SiteGroupMember
                    .Include(item => item.SiteGroup) // eager load
                    .FirstOrDefault(item => item.SiteGroupMemberId == siteGroupMemberId);
            }
            else
            {
                return db.SiteGroupMember.AsNoTracking()
                    .Include(item => item.SiteGroup) // eager load 
                    .FirstOrDefault(item => item.SiteGroupMemberId == siteGroupMemberId);
            }
        }

        public void DeleteSiteGroupMember(int siteGroupMemberId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            SiteGroupMember SiteGroupMember = db.SiteGroupMember.Find(siteGroupMemberId);
            db.SiteGroupMember.Remove(SiteGroupMember);
            db.SaveChanges();
        }
    }
}

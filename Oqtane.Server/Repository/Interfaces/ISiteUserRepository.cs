using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteUserRepository
    {
        IEnumerable<SiteUser> GetSiteUsers();
        IEnumerable<SiteUser> GetSiteUsers(int SiteId);
        SiteUser AddSiteUser(SiteUser SiteUser);
        SiteUser UpdateSiteUser(SiteUser SiteUser);
        SiteUser GetSiteUser(int SiteUserId);
        SiteUser GetSiteUser(int SiteId, int UserId);
        void DeleteSiteUser(int SiteUserId);
    }
}

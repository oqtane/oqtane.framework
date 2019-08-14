using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface ISiteUserRepository
    {
        IEnumerable<SiteUser> GetSiteUsers();
        IEnumerable<SiteUser> GetSiteUsers(int SiteId, int UserId);
        SiteUser AddSiteUser(SiteUser SiteUser);
        SiteUser UpdateSiteUser(SiteUser SiteUser);
        SiteUser GetSiteUser(int SiteUserId);
        void DeleteSiteUser(int SiteUserId);
    }
}

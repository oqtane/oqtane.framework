using System.Collections.Generic;
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
}

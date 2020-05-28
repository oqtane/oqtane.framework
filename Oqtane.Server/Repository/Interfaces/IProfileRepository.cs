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
        void DeleteProfile(int profileId);
    }
}

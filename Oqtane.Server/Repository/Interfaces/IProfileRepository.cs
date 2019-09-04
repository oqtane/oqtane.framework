using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IProfileRepository
    {
        IEnumerable<Profile> GetProfiles();
        IEnumerable<Profile> GetProfiles(int SiteId);
        Profile AddProfile(Profile Profile);
        Profile UpdateProfile(Profile Profile);
        Profile GetProfile(int ProfileId);
        void DeleteProfile(int ProfileId);
    }
}

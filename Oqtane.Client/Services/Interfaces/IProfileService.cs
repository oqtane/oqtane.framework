using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IProfileService
    {
        Task<List<Profile>> GetProfilesAsync(int siteId);

        Task<Profile> GetProfileAsync(int profileId);

        Task<Profile> AddProfileAsync(Profile profile);

        Task<Profile> UpdateProfileAsync(Profile profile);

        Task DeleteProfileAsync(int profileId);
    }
}

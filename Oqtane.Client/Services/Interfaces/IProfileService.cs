using System.Collections.Generic;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Services.Interfaces
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

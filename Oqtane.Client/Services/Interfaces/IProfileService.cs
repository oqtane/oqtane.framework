using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public interface IProfileService
    {
        Task<List<Profile>> GetProfilesAsync();

        Task<List<Profile>> GetProfilesAsync(int SiteId);

        Task<Profile> GetProfileAsync(int ProfileId);

        Task<Profile> AddProfileAsync(Profile Profile);

        Task<Profile> UpdateProfileAsync(Profile Profile);

        Task DeleteProfileAsync(int ProfileId);
    }
}

using Oqtane.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to store and retreive <see cref="Profile"/> entries
    /// </summary>
    public interface IProfileService
    {

        /// <summary>
        /// Returns a list of profile entries
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Task<List<Profile>> GetProfilesAsync(int siteId);

        /// <summary>
        /// Returns a specific profile entry
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        Task<Profile> GetProfileAsync(int profileId);

        /// <summary>
        /// Creates a new profile entry
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        Task<Profile> AddProfileAsync(Profile profile);

        /// <summary>
        /// Updates an existing profile entry
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        Task<Profile> UpdateProfileAsync(Profile profile);

        /// <summary>
        /// Deletes a profile entry
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
        Task DeleteProfileAsync(int profileId);
    }
}

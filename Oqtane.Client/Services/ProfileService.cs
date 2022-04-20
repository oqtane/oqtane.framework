using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class ProfileService : ServiceBase, IProfileService
    {
        public ProfileService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Profile");

        public async Task<List<Profile>> GetProfilesAsync(int siteId)
        {
            List<Profile> profiles = await GetJsonAsync<List<Profile>>($"{Apiurl}?siteid={siteId}");
            return profiles.OrderBy(item => item.ViewOrder).ToList();
        }

        public async Task<Profile> GetProfileAsync(int profileId)
        {
            return await GetJsonAsync<Profile>($"{Apiurl}/{profileId}");
        }

        public async Task<Profile> AddProfileAsync(Profile profile)
        {
            return await PostJsonAsync<Profile>(Apiurl, profile);
        }

        public async Task<Profile> UpdateProfileAsync(Profile profile)
        {
            return await PutJsonAsync<Profile>($"{Apiurl}/{profile.SiteId}", profile);
        }
        public async Task DeleteProfileAsync(int profileId)
        {
            await DeleteAsync($"{Apiurl}/{profileId}");
        }
    }
}

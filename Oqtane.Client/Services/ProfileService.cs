using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using Oqtane.Services.Interfaces;
using Oqtane.Shared;

namespace Oqtane.Services
{
    public class ProfileService : ServiceBase, IProfileService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public ProfileService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Profile"); }
        }

        public async Task<List<Profile>> GetProfilesAsync(int siteId)
        {
            List<Profile> profiles = await _http.GetJsonAsync<List<Profile>>($"{Apiurl}?siteid={siteId.ToString()}");
            return profiles.OrderBy(item => item.ViewOrder).ToList();
        }

        public async Task<Profile> GetProfileAsync(int profileId)
        {
            return await _http.GetJsonAsync<Profile>($"{Apiurl}/{profileId.ToString()}");
        }

        public async Task<Profile> AddProfileAsync(Profile profile)
        {
            return await _http.PostJsonAsync<Profile>(Apiurl, profile);
        }

        public async Task<Profile> UpdateProfileAsync(Profile profile)
        {
            return await _http.PutJsonAsync<Profile>($"{Apiurl}/{profile.SiteId.ToString()}", profile);
        }
        public async Task DeleteProfileAsync(int profileId)
        {
            await _http.DeleteAsync($"{Apiurl}/{profileId.ToString()}");
        }
    }
}

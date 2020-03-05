using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
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

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Profile"); }
        }

        public async Task<List<Profile>> GetProfilesAsync(int SiteId)
        {
            List<Profile> Profiles = await _http.GetJsonAsync<List<Profile>>(apiurl + "?siteid=" + SiteId.ToString());
            return Profiles.OrderBy(item => item.ViewOrder).ToList();
        }

        public async Task<Profile> GetProfileAsync(int ProfileId)
        {
            return await _http.GetJsonAsync<Profile>(apiurl + "/" + ProfileId.ToString());
        }

        public async Task<Profile> AddProfileAsync(Profile Profile)
        {
            return await _http.PostJsonAsync<Profile>(apiurl, Profile);
        }

        public async Task<Profile> UpdateProfileAsync(Profile Profile)
        {
            return await _http.PutJsonAsync<Profile>(apiurl + "/" + Profile.SiteId.ToString(), Profile);
        }
        public async Task DeleteProfileAsync(int ProfileId)
        {
            await _http.DeleteAsync(apiurl + "/" + ProfileId.ToString());
        }
    }
}

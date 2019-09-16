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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public ProfileService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Profile"); }
        }

        public async Task<List<Profile>> GetProfilesAsync()
        {
            return await http.GetJsonAsync<List<Profile>>(apiurl);
        }

        public async Task<List<Profile>> GetProfilesAsync(int SiteId)
        {
            List<Profile> Profiles = await http.GetJsonAsync<List<Profile>>(apiurl + "?siteid=" + SiteId.ToString());
            return Profiles.OrderBy(item => item.ViewOrder).ToList();
        }

        public async Task<Profile> GetProfileAsync(int ProfileId)
        {
            return await http.GetJsonAsync<Profile>(apiurl + "/" + ProfileId.ToString());
        }

        public async Task<Profile> AddProfileAsync(Profile Profile)
        {
            return await http.PostJsonAsync<Profile>(apiurl, Profile);
        }

        public async Task<Profile> UpdateProfileAsync(Profile Profile)
        {
            return await http.PutJsonAsync<Profile>(apiurl + "/" + Profile.SiteId.ToString(), Profile);
        }
        public async Task DeleteProfileAsync(int ProfileId)
        {
            await http.DeleteAsync(apiurl + "/" + ProfileId.ToString());
        }
    }
}

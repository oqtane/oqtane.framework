using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System.Linq;

namespace Oqtane.Services
{
    public class PackageService : ServiceBase, IPackageService
    {
        private readonly HttpClient _http;
        private readonly SiteState _siteState;
        private readonly NavigationManager _navigationManager;

        public PackageService(HttpClient http, SiteState siteState, NavigationManager navigationManager)
        {
            _http = http;
            _siteState = siteState;
            _navigationManager = navigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Package"); }
        }

        public async Task<List<Package>> GetPackagesAsync(string Tag)
        {
            List<Package> packages = await _http.GetJsonAsync<List<Package>>(apiurl + "?tag=" + Tag);
            return packages.OrderByDescending(item => item.Downloads).ToList();
        }

        public async Task DownloadPackageAsync(string PackageId, string Version, string Folder)
        {
            await _http.PostJsonAsync(apiurl + "?packageid=" + PackageId + "&version=" + Version + "&folder=" + Folder, null);
        }
    }
}

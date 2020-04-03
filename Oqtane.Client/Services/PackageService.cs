using Oqtane.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System.Linq;
using Oqtane.Services.Interfaces;

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

        private string Apiurl
        {
            get { return CreateApiUrl(_siteState.Alias, _navigationManager.Uri, "Package"); }
        }

        public async Task<List<Package>> GetPackagesAsync(string tag)
        {
            List<Package> packages = await _http.GetJsonAsync<List<Package>>($"{Apiurl}?tag={tag}");
            return packages.OrderByDescending(item => item.Downloads).ToList();
        }

        public async Task DownloadPackageAsync(string packageId, string version, string folder)
        {
            await _http.PostJsonAsync($"{Apiurl}?packageid={packageId}&version={version}&folder={folder}", null);
        }
    }
}

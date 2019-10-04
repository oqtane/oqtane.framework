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
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly NavigationManager NavigationManager;

        public PackageService(HttpClient http, SiteState sitestate, NavigationManager NavigationManager)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.NavigationManager = NavigationManager;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, NavigationManager.Uri, "Package"); }
        }

        public async Task<List<Package>> GetPackagesAsync(string Tag)
        {
            List<Package> packages = await http.GetJsonAsync<List<Package>>(apiurl + "?tag=" + Tag);
            return packages.OrderByDescending(item => item.Downloads).ToList();
        }

        public async Task DownloadPackageAsync(string PackageId, string Version, string Folder)
        {
            await http.PostJsonAsync(apiurl + "?packageid=" + PackageId + "&version=" + Version + "&folder=" + Folder, null);
        }
    }
}

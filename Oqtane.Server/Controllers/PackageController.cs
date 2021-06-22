using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Enums;
// ReSharper disable PartialTypeWithSinglePart

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class PackageController : Controller
    {
        private readonly IInstallationManager _installationManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfigManager _configManager;
        private readonly ILogManager _logger;

        public PackageController(IInstallationManager installationManager, IWebHostEnvironment environment, IConfigManager configManager, ILogManager logger)
        {
            _installationManager = installationManager;
            _environment = environment;
            _configManager = configManager;
            _logger = logger;
        }

        // GET: api/<controller>?type=x&search=y
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public async Task<IEnumerable<Package>> Get(string type, string search)
        {
            // get packages
            List<Package> packages;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Host.Value);
                packages = await GetJson<List<Package>>(client, Constants.PackageRegistryUrl + $"/api/registry/packages/?installationid={GetInstallationId()}&type={type.ToLower()}&version={Constants.Version}&search={search}");
            }
            return packages;
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public async Task Post(string packageid, string version, string folder)
        {
            // get package info
            Package package = null;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Host.Value);
                package = await GetJson<Package>(client, Constants.PackageRegistryUrl + $"/api/registry/package/?installationid={GetInstallationId()}&packageid={packageid}&version={version}");
            }

            if (package != null)
            {
                using (var httpClient = new HttpClient())
                {
                    folder = Path.Combine(_environment.ContentRootPath, folder);
                    var response = await httpClient.GetAsync(package.PackageUrl).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string filename = packageid + "." + version + ".nupkg";
                    using (var fileStream = new FileStream(Path.Combine(folder, filename), FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }
            }
        }

        private string GetInstallationId()
        {
            var installationid = _configManager.GetSetting("InstallationId", "");
            if (installationid == "")
            {
                installationid = Guid.NewGuid().ToString();
                _configManager.AddOrUpdateSetting("InstallationId", installationid, true);
            }
            return installationid;
        }

        private async Task<T> GetJson<T>(HttpClient httpClient, string url)
        {
            Uri uri = new Uri(url);
            var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            using (var streamReader = new StreamReader(stream))
            {
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    return serializer.Deserialize<T>(jsonTextReader);
                }
            }
        }

        [HttpGet("install")]
        [Authorize(Roles = RoleNames.Host)]
        public void InstallPackages()
        {
            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Packages Installed");
            _installationManager.InstallPackages();
        }
    }

    public partial class SearchResult
    {
        [JsonProperty("@context")]
        public Context Context { get; set; }

        [JsonProperty("totalHits")]
        public long TotalHits { get; set; }

        [JsonProperty("data")]
        public Data[] Data { get; set; }
    }

    public partial class Context
    {
        [JsonProperty("@vocab")]
        public Uri Vocab { get; set; }

        [JsonProperty("@base")]
        public Uri Base { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("@id")]
        public Uri Url { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("registration")]
        public Uri Registration { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("iconUrl")]
        public Uri IconUrl { get; set; }

        [JsonProperty("licenseUrl")]
        public Uri LicenseUrl { get; set; }

        [JsonProperty("projectUrl")]
        public Uri ProjectUrl { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("authors")]
        public string[] Authors { get; set; }

        [JsonProperty("totalDownloads")]
        public long TotalDownloads { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }

        [JsonProperty("versions")]
        public Version[] Versions { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("version")]
        public string Number { get; set; }

        [JsonProperty("downloads")]
        public long Downloads { get; set; }

        [JsonProperty("@id")]
        public Uri Url { get; set; }
    }
}

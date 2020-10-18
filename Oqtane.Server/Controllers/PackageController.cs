using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
// ReSharper disable PartialTypeWithSinglePart

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class PackageController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public PackageController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // GET: api/<controller>?tag=x
        [HttpGet]
        [Authorize(Roles = RoleNames.Host)]
        public async Task<IEnumerable<Package>> Get(string tag)
        {
            List<Package> packages = new List<Package>();

            using (var httpClient = new HttpClient())
            {
                var searchResult = await GetJson<SearchResult>(httpClient, "https://azuresearch-usnc.nuget.org/query?q=tags:oqtane");
                foreach(Data data in searchResult.Data)
                {
                    if (data.Tags.Contains(tag))
                    {
                        Package package = new Package();
                        package.PackageId = data.Id;
                        package.Name = data.Title;
                        package.Description = data.Description;
                        package.Owner = data.Authors[0];
                        package.Version = data.Version;
                        package.Downloads = data.TotalDownloads;
                        packages.Add(package);
                    }
                }
            }

            return packages;
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public async Task Post(string packageid, string version, string folder)
        {
            using (var httpClient = new HttpClient())
            {
                folder = Path.Combine(_environment.WebRootPath, folder);
                var response = await httpClient.GetAsync("https://www.nuget.org/api/v2/package/" + packageid.ToLower() + "/" + version).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string filename = packageid + "." + version + ".nupkg";
                using (var fileStream = new FileStream(Path.Combine(folder, filename), FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
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

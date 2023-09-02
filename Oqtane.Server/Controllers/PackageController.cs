using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Enums;
using System.Net.Http.Headers;
using System.Text.Json;
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

        // GET: api/<controller>?type=x&search=y&price=z&package=a
        [HttpGet]
        public async Task<IEnumerable<Package>> Get(string type, string search, string price, string package, string sort)
        {
            // get packages
            List<Package> packages = new List<Package>();
            var url = _configManager.GetSetting("PackageRegistryUrl", Constants.PackageRegistryUrl);
            if (!string.IsNullOrEmpty(url))
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    packages = await GetJson<List<Package>>(client, url + $"/api/registry/packages/?id={_configManager.GetInstallationId()}&type={type.ToLower()}&version={Constants.Version}&search={search}&price={price}&package={package}&sort={sort}");
                }
            }
            return packages;
        }

        // GET: api/<controller>/updates/?type=x
        [HttpGet("updates")]
        public async Task<IEnumerable<Package>> GetPackageUpdates(string type)
        {
            // get packages
            List<Package> packages = new List<Package>();
            var url = _configManager.GetSetting("PackageRegistryUrl", Constants.PackageRegistryUrl);
            if (!string.IsNullOrEmpty(url))
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    packages = await GetJson<List<Package>>(client, url + $"/api/registry/updates/?id={_configManager.GetInstallationId()}&version={Constants.Version}&type={type}");
                }
            }
            return packages;
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public async Task<Package> Post(string packageid, string version, string download, string install)
        {
            // get package info
            Package package = null;
            var url = _configManager.GetSetting("PackageRegistryUrl", Constants.PackageRegistryUrl);
            if (!string.IsNullOrEmpty(url))
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    package = await GetJson<Package>(client, url + $"/api/registry/package/?id={_configManager.GetInstallationId()}&package={packageid}&version={version}&download={download}");
                }

                if (package != null)
                {
                    if (bool.Parse(install))
                    {
                        using (var httpClient = new HttpClient())
                        {
                            var folder = Path.Combine(_environment.ContentRootPath, Constants.PackagesFolder);
                            var response = await httpClient.GetAsync(package.PackageUrl).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                string filename = packageid + "." + version + ".nupkg";
                                using (var fileStream = new FileStream(Path.Combine(Constants.PackagesFolder, filename), FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    await response.Content.CopyToAsync(fileStream).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, this, LogFunction.Create, "Could Not Download {PackageUrl}", package.PackageUrl);
                            }
                        }
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "Package {PackageId}.{Version} Is Not Registered In The Marketplace", packageid, version);
                }
            }
            return package;
        }

        private async Task<T> GetJson<T>(HttpClient httpClient, string url)
        {
            try
            {
                Uri uri = new Uri(url);
                var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
                if (response.IsSuccessStatusCode && ValidateJsonContent(response.Content))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    using (var streamReader = new StreamReader(stream))
                    {
                        return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                    }
                }
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, ex, "Error Accessing Marketplace API {Url}", url);
                return default(T);
            }
        }

        private static bool ValidateJsonContent(HttpContent content)
        {
            var mediaType = content?.Headers.ContentType?.MediaType;
            return mediaType != null && mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet("install")]
        [Authorize(Roles = RoleNames.Host)]
        public void InstallPackages()
        {
            _logger.Log(LogLevel.Information, this, LogFunction.Create, "Packages Installed");
            _installationManager.InstallPackages();
        }
    }
}

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
        public async Task<IEnumerable<Package>> Get(string type, string search, string price, string package)
        {
            // get packages
            List<Package> packages = new List<Package>();
            if (bool.Parse(_configManager.GetSetting("PackageService", "true")) == true)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    packages = await GetJson<List<Package>>(client, Constants.PackageRegistryUrl + $"/api/registry/packages/?id={_configManager.GetInstallationId()}&type={type.ToLower()}&version={Constants.Version}&search={search}&price={price}&package={package}");
                }
            }
            return packages;
        }

        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public async Task<Package> Post(string packageid, string version, string folder)
        {
            // get package info
            Package package = null;
            if (bool.Parse(_configManager.GetSetting("PackageService", "true")) == true)
            {
                var download = (string.IsNullOrEmpty(folder)) ? "false" : "true";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Referer", HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value);
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Constants.PackageId, Constants.Version));
                    package = await GetJson<Package>(client, Constants.PackageRegistryUrl + $"/api/registry/package/?id={_configManager.GetInstallationId()}&package={packageid}&version={version}&download={download}");
                }

                if (package != null)
                {
                    if (bool.Parse(download))
                    {
                        using (var httpClient = new HttpClient())
                        {
                            folder = Path.Combine(_environment.ContentRootPath, folder);
                            var response = await httpClient.GetAsync(package.PackageUrl).ConfigureAwait(false);
                            if (response.IsSuccessStatusCode)
                            {
                                string filename = packageid + "." + version + ".nupkg";
                                using (var fileStream = new FileStream(Path.Combine(folder, filename), FileMode.Create, FileAccess.Write, FileShare.None))
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
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "Package {PackageId}.{Version} Is Not Registered", packageid, version);
                }
            }
            return package;
        }

        private async Task<T> GetJson<T>(HttpClient httpClient, string url)
        {
            Uri uri = new Uri(url);
            var response = await httpClient.GetAsync(uri).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using (var streamReader = new StreamReader(stream))
                {
                    return await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                }
            }
            return default(T);
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

using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.Net;
using System.Linq;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage (install master database / upgrade version / etc.) the installation
    /// </summary>
    public interface IInstallationService
    {
        /// <summary>
        /// Returns a status/message object with the current installation state 
        /// </summary>
        /// <returns></returns>
        Task<Installation> IsInstalled();

        /// <summary>
        /// Starts the installation process 
        /// </summary>
        /// <param name="config">connectionString, database type, alias etc.</param>
        /// <returns>internal status/message object</returns>
        Task<Installation> Install(InstallConfig config);

        /// <summary>
        /// Starts the upgrade process
        /// </summary>
        /// <param name="backup">indicates if files should be backed up during upgrade</param>
        /// <returns>internal status/message object</returns>
        Task<Installation> Upgrade(bool backup);

        /// <summary>
        /// Restarts the installation
        /// </summary>
        /// <returns>internal status/message object</returns>
        Task RestartAsync();
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class InstallationService : ServiceBase, IInstallationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly SiteState _siteState;
        private readonly HttpClient _http;

        public InstallationService(HttpClient http, SiteState siteState, NavigationManager navigationManager) : base(http, siteState)
        {
            _navigationManager = navigationManager;
            _siteState = siteState;
            _http = http;
        }

        private string ApiUrl => (_siteState.Alias == null)
            ? CreateApiUrl("Installation", null, ControllerRoutes.ApiRoute) // tenant agnostic needed for initial installation
            : CreateApiUrl("Installation", _siteState.Alias); 

        public async Task<Installation> IsInstalled()
        {
            var path = "";
            if (_http.DefaultRequestHeaders.UserAgent.ToString().Contains(Constants.MauiUserAgent))
            {
                path = _http.DefaultRequestHeaders.GetValues(Constants.MauiAliasPath).First();
            }
            else
            {
                path = new Uri(_navigationManager.Uri).LocalPath.Substring(1);
            }
            return await GetJsonAsync<Installation>($"{ApiUrl}/installed/?path={WebUtility.UrlEncode(path)}");
        }

        public async Task<Installation> Install(InstallConfig config)
        {
            return await PostJsonAsync<InstallConfig,Installation>(ApiUrl, config);
        }

        public async Task<Installation> Upgrade(bool backup)
        {
            return await GetJsonAsync<Installation>($"{ApiUrl}/upgrade/?backup={backup}");
        }

        public async Task RestartAsync()
        {
            await PostAsync($"{ApiUrl}/restart");
        }
    }
}

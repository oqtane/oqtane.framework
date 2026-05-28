using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage tasks (<see cref="SiteTask"/>)
    /// </summary>
    public interface ISiteTaskService
    {
        /// <summary>
        /// Return a specific task
        /// </summary>
        /// <param name="siteTaskId"></param>
        /// <returns></returns>
        Task<SiteTask> GetSiteTaskAsync(int siteTaskId);

        /// <summary>
        /// Adds a new task
        /// </summary>
        /// <param name="siteTask"></param>
        /// <returns></returns>
        Task<SiteTask> AddSiteTaskAsync(SiteTask siteTask);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteTaskService : ServiceBase, ISiteTaskService
    {
        public SiteTaskService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("SiteTask");

        public async Task<SiteTask> GetSiteTaskAsync(int siteTaskId)
        {
            return await GetJsonAsync<SiteTask>($"{Apiurl}/{siteTaskId}");
        }

        public async Task<SiteTask> AddSiteTaskAsync(SiteTask siteTask)
        {
            return await PostJsonAsync<SiteTask>(Apiurl, siteTask);
        }
    }
}

using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class JobLogService : ServiceBase, IJobLogService
    {
        public JobLogService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("JobLog");

        public async Task<List<JobLog>> GetJobLogsAsync(int jobId)
        {
            return await GetJsonAsync<List<JobLog>>($"{Apiurl}?jobid={jobId}");
        }

        public async Task<JobLog> GetJobLogAsync(int jobLogId)
        {
            return await GetJsonAsync<JobLog>($"{Apiurl}/{jobLogId}");
        }
    }
}

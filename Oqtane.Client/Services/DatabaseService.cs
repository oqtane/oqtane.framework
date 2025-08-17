using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to retrieve <see cref="Database"/> information.
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Returns a list of databases
        /// </summary>
        /// <returns></returns>
        Task<List<Database>> GetDatabasesAsync();
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class DatabaseService : ServiceBase, IDatabaseService
    {
        public DatabaseService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("Database");

        public async Task<List<Database>> GetDatabasesAsync()
        {
            List<Database> databases = await GetJsonAsync<List<Database>>(Apiurl);
            return databases.OrderBy(item => item.Name).ToList();
        }
    }
}

using Oqtane.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    public class SqlService : ServiceBase, ISqlService
    {
        public SqlService(HttpClient http) : base(http) { }

        private string Apiurl => CreateApiUrl("Sql");

        public async Task<SqlQuery> ExecuteQueryAsync(SqlQuery sqlquery)
        {
            return await PostJsonAsync<SqlQuery>(Apiurl, sqlquery);
        }
    }
}

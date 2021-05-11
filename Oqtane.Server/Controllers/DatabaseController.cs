using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Shared;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class DatabaseController : Controller
    {
        public DatabaseController() { }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<Models.Database> Get()
        {
            var databases = new List<Models.Database>
            {
                new()
                {
                    Name = "LocalDB",
                    FriendlyName = "Local Database",
                    Type = "Oqtane.Installer.Controls.LocalDBConfig, Oqtane.Client"
                },
                new()
                {
                    Name = "SqlServer",
                    FriendlyName = "SQL Server",
                    Type = "Oqtane.Installer.Controls.SqlServerConfig, Oqtane.Client"
                },
                new()
                {
                    Name = "Sqlite",
                    FriendlyName = "Sqlite",
                    Type = "Oqtane.Installer.Controls.SqliteConfig, Oqtane.Client"
                },
                new()
                {
                    Name = "MySQL",
                    FriendlyName = "MySQL",
                    Type = "Oqtane.Installer.Controls.MySQLConfig, Oqtane.Client"
                },
                new()
                {
                    Name = "PostgreSQL",
                    FriendlyName = "PostgreSQL",
                    Type = "Oqtane.Installer.Controls.PostGreSQLConfig, Oqtane.Client"
                }
            };
            return databases;
        }
    }
}

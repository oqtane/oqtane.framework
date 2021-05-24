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
                    ControlType = "Oqtane.Installer.Controls.LocalDBConfig, Oqtane.Client",
                    DBType = "Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer",
                    Package = "Oqtane.Database.SqlServer"
                },
                new()
                {
                    Name = "SqlServer",
                    FriendlyName = "SQL Server",
                    ControlType = "Oqtane.Installer.Controls.SqlServerConfig, Oqtane.Client",
                    DBType = "Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer",
                    Package = "Oqtane.Database.SqlServer"
                },
                new()
                {
                    Name = "Sqlite",
                    FriendlyName = "Sqlite",
                    ControlType = "Oqtane.Installer.Controls.SqliteConfig, Oqtane.Client",
                    DBType = "Oqtane.Database.Sqlite.SqliteDatabase, Oqtane.Database.Sqlite",
                    Package = "Oqtane.Database.Sqlite"
                },
                new()
                {
                    Name = "MySQL",
                    FriendlyName = "MySQL",
                    ControlType = "Oqtane.Installer.Controls.MySQLConfig, Oqtane.Client",
                    DBType = "Oqtane.Database.MySQL.MySQLDatabase, Oqtane.Database.MySQL",
                    Package = "Oqtane.Database.MySQL"
                },
                new()
                {
                    Name = "PostgreSQL",
                    FriendlyName = "PostgreSQL",
                    ControlType = "Oqtane.Installer.Controls.PostgreSQLConfig, Oqtane.Client",
                    DBType = "Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Database.PostgreSQL",
                    Package = "Oqtane.Database.PostgreSQL"
                }
            };
            return databases;
        }
    }
}

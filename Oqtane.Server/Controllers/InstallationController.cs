using DbUp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class InstallationController : Controller
    {
        private readonly IConfigurationRoot Config;
        private readonly IInstallationManager InstallationManager;

        public InstallationController(IConfigurationRoot Config, IInstallationManager InstallationManager)
        {
            this.Config = Config;
            this.InstallationManager = InstallationManager;
        }

        // POST api/<controller>
        [HttpPost]
        public GenericResponse Post([FromBody] string connectionstring)
        {
            var response = new GenericResponse { Success = false, Message = "" };

            if (ModelState.IsValid)
            {
                bool master = false;
                string defaultconnectionstring = Config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(defaultconnectionstring) || connectionstring == defaultconnectionstring)
                {
                    master = true;
                }

                bool exists = false;
                if (master)
                {
                    exists = IsInstalled().Success;
                }

                if (!exists)
                {
                    string datadirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
                    connectionstring = connectionstring.Replace("|DataDirectory|", datadirectory);

                    SqlConnection connection = new SqlConnection(connectionstring);
                    try
                    {
                        using (connection)
                        {
                            connection.Open();
                        }
                        exists = true;
                    }
                    catch
                    {
                        // database does not exist
                    }

                    // try to create database if it does not exist
                    if (!exists)
                    {
                        string masterConnectionString = "";
                        string databaseName = "";
                        string[] fragments = connectionstring.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        foreach (string fragment in fragments)
                        {
                            if (fragment.ToLower().Contains("initial catalog=") || fragment.ToLower().Contains("database="))
                            {
                                databaseName = fragment.Substring(fragment.IndexOf("=") + 1);
                            }
                            else
                            {
                                if (!fragment.ToLower().Contains("attachdbfilename="))
                                {
                                    masterConnectionString += fragment + ";";
                                }
                            }
                        }
                        connection = new SqlConnection(masterConnectionString);
                        try
                        {
                            using (connection)
                            {
                                connection.Open();
                                SqlCommand command;
                                if (connectionstring.ToLower().Contains("attachdbfilename=")) // LocalDB
                                {
                                    command = new SqlCommand("CREATE DATABASE [" + databaseName + "] ON ( NAME = '" + databaseName + "', FILENAME = '" + datadirectory + "\\" + databaseName + ".mdf')", connection);
                                }
                                else
                                {
                                    command = new SqlCommand("CREATE DATABASE [" + databaseName + "]", connection);
                                }
                                command.ExecuteNonQuery();
                                exists = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            response.Message = "Can Not Create Database - " + ex.Message;
                        }

                        // sleep to allow SQL server to attach new database
                        Thread.Sleep(5000);
                    }

                    if (exists)
                    {
                        // get master initialization script and update connectionstring and alias in seed data
                        string initializationScript = "";
                        if (master)
                        {
                            using (StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\Scripts\\Master.sql"))
                            {
                                initializationScript = reader.ReadToEnd();
                            }
                            initializationScript = initializationScript.Replace("{ConnectionString}", connectionstring.Replace(datadirectory, "|DataDirectory|"));
                            initializationScript = initializationScript.Replace("{Alias}", HttpContext.Request.Host.Value);
                        }

                        var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionstring)
                            .WithScript(new DbUp.Engine.SqlScript("Master.sql", initializationScript))
                            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly()); // tenant scripts should be added to /Scripts folder as Embedded Resources
                        var dbUpgrade = dbUpgradeConfig.Build();
                        if (dbUpgrade.IsUpgradeRequired())
                        {
                            var result = dbUpgrade.PerformUpgrade();
                            if (!result.Successful)
                            {
                                response.Message = result.Error.Message;
                            }
                            else
                            {
                                // update appsettings
                                if (master)
                                {
                                    string config = "";
                                    using (StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\appsettings.json"))
                                    {
                                        config = reader.ReadToEnd();
                                    }
                                    connectionstring = connectionstring.Replace(datadirectory, "|DataDirectory|");
                                    connectionstring = connectionstring.Replace(@"\", @"\\");
                                    config = config.Replace("DefaultConnection\": \"", "DefaultConnection\": \"" + connectionstring);
                                    using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\appsettings.json"))
                                    {
                                        writer.WriteLine(config);
                                    }
                                    Config.Reload();
                                }
                                response.Success = true;
                            }
                        }
                    }
                }
                else
                {
                    response.Message = "Application Is Already Installed";
                }
            }
            return response;
        }

        // GET api/<controller>/installed
        [HttpGet("installed")]
        public GenericResponse IsInstalled()
        {
            var response = new GenericResponse { Success = false, Message = "" };

            string datadirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string connectionString = Config.GetConnectionString("DefaultConnection");
            connectionString = connectionString.Replace("|DataDirectory|", datadirectory);

            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                using (connection)
                {
                    connection.Open();
                }
                response.Success = true;
            }
            catch
            {
                // database does not exist
                response.Message = "Database Does Not Exist";
            }

            if (response.Success)
            {
                var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                    .WithScript(new DbUp.Engine.SqlScript("Master.sql", ""));
                var dbUpgrade = dbUpgradeConfig.Build();
                response.Success = !dbUpgrade.IsUpgradeRequired();
                if (!response.Success)
                {
                    response.Message = "Master Installation Scripts Have Not Been Executed";
                }
                else
                {
                    using (var db = new InstallationContext(connectionString))
                    {
                        ApplicationVersion version = db.ApplicationVersion.ToList().LastOrDefault();
                        if (version == null || version.Version != Constants.Version)
                        {
                            version = new ApplicationVersion();
                            version.Version = Constants.Version;
                            version.CreatedOn = DateTime.Now;
                            db.ApplicationVersion.Add(version);
                            db.SaveChanges();
                        }
                    }

                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(item => item.FullName.Contains(".Module.")).ToArray();

                    // get tenants
                    using (var db = new InstallationContext(connectionString))
                    {
                        foreach (Tenant tenant in db.Tenant.ToList())
                        {
                            connectionString = tenant.DBConnectionString;
                            connectionString = connectionString.Replace("|DataDirectory|", datadirectory);

                            // upgrade framework
                            dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly());
                            dbUpgrade = dbUpgradeConfig.Build();
                            if (dbUpgrade.IsUpgradeRequired())
                            {
                                var result = dbUpgrade.PerformUpgrade();
                                if (!result.Successful)
                                {
                                    // TODO: log result.Error.Message;
                                }
                            }
                            // iterate through Oqtane module assemblies and execute any database scripts
                            foreach (Assembly assembly in assemblies)
                            {
                                InstallModule(assembly, connectionString);
                            }
                        }
                    }
                }
            }

            return response;
        }

        private void InstallModule(Assembly assembly, string connectionstring)
        {
            var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionstring)
                .WithScriptsEmbeddedInAssembly(assembly); // scripts must be included as Embedded Resources
            var dbUpgrade = dbUpgradeConfig.Build();
            if (dbUpgrade.IsUpgradeRequired())
            {
                var result = dbUpgrade.PerformUpgrade();
                if (!result.Successful)
                {
                    // TODO: log result.Error.Message;
                }
            }
        }

        [HttpGet("upgrade")]
        [Authorize(Roles = Constants.HostRole)]
        public GenericResponse Upgrade()
        {
            var response = new GenericResponse { Success = true, Message = "" };
            InstallationManager.UpgradeFramework();
            return response;
        }
    }

    public class InstallationContext : DbContext
    {
        private readonly string _connectionString;

        public InstallationContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
           => optionsBuilder.UseSqlServer(_connectionString);

        public virtual DbSet<ApplicationVersion> ApplicationVersion { get; set; }
        public virtual DbSet<Tenant> Tenant { get; set; }
    }
}

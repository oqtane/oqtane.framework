using DbUp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Oqtane.Infrastructure.Interfaces;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class InstallationController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly IInstallationManager _installationManager;

        public InstallationController(IConfigurationRoot config, IInstallationManager installationManager)
        {
            _config = config;
            _installationManager = installationManager;
        }

        // POST api/<controller>
        [HttpPost]
        public Installation Post([FromBody] string connectionString)
        {
            var installation = new Installation { Success = false, Message = "" };

            if (ModelState.IsValid)
            {
                bool master = false;
                string defaultconnectionstring = _config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(defaultconnectionstring) || connectionString == defaultconnectionstring)
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
                    connectionString = connectionString.Replace("|DataDirectory|", datadirectory);

                    SqlConnection connection = new SqlConnection(connectionString);
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
                        string[] fragments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
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
                                if (connectionString.ToLower().Contains("attachdbfilename=")) // LocalDB
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
                            installation.Message = "Can Not Create Database - " + ex.Message;
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
                            initializationScript = initializationScript.Replace("{ConnectionString}", connectionString.Replace(datadirectory, "|DataDirectory|"));
                            initializationScript = initializationScript.Replace("{Alias}", HttpContext.Request.Host.Value);
                        }

                        var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                            .WithScript(new DbUp.Engine.SqlScript("Master.sql", initializationScript))
                            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly()); // tenant scripts should be added to /Scripts folder as Embedded Resources
                        var dbUpgrade = dbUpgradeConfig.Build();
                        if (dbUpgrade.IsUpgradeRequired())
                        {
                            var result = dbUpgrade.PerformUpgrade();
                            if (!result.Successful)
                            {
                                installation.Message = result.Error.Message;
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
                                    connectionString = connectionString.Replace(datadirectory, "|DataDirectory|");
                                    connectionString = connectionString.Replace(@"\", @"\\");
                                    config = config.Replace("DefaultConnection\": \"", "DefaultConnection\": \"" + connectionString);
                                    using (StreamWriter writer = new StreamWriter(Directory.GetCurrentDirectory() + "\\appsettings.json"))
                                    {
                                        writer.WriteLine(config);
                                    }
                                    _config.Reload();
                                }
                                installation.Success = true;
                            }
                        }
                    }
                }
                else
                {
                    installation.Message = "Application Is Already Installed";
                }
            }
            return installation;
        }

        // GET api/<controller>/installed
        [HttpGet("installed")]
        public Installation IsInstalled()
        {
            var installation = new Installation { Success = false, Message = "" };

            string datadirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string connectionString = _config.GetConnectionString("DefaultConnection");
            connectionString = connectionString.Replace("|DataDirectory|", datadirectory);

            if (!string.IsNullOrEmpty(connectionString))
            {
                SqlConnection connection = new SqlConnection(connectionString);
                try
                {
                    using (connection)
                    {
                        connection.Open();
                    }
                    installation.Success = true;
                }
                catch
                {
                    // database does not exist
                    installation.Message = "Database Does Not Exist";
                }
            }
            else
            {
                installation.Message = "Connection String Has Not Been Specified In Oqtane.Server\\appsettings.json";
            }

            if (installation.Success)
            {
                var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                    .WithScript(new DbUp.Engine.SqlScript("Master.sql", ""));
                var dbUpgrade = dbUpgradeConfig.Build();
                installation.Success = !dbUpgrade.IsUpgradeRequired();
                if (!installation.Success)
                {
                    installation.Message = "Master Installation Scripts Have Not Been Executed";
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
                            version.CreatedOn = DateTime.UtcNow;
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
                                    // TODO: log result.Error.Message - problem is logger is not available here
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

            return installation;
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
                    // TODO: log result.Error.Message - problem is logger is not available here
                }
            }
        }

        [HttpGet("upgrade")]
        [Authorize(Roles = Constants.HostRole)]
        public Installation Upgrade()
        {
            var installation = new Installation { Success = true, Message = "" };
            _installationManager.UpgradeFramework();
            return installation;
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

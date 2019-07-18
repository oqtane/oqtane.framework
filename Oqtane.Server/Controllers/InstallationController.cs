using DbUp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oqtane.Models;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class InstallationController : Controller
    {
        private readonly IConfigurationRoot _config;

        public InstallationController(IConfigurationRoot config)
        {
            _config = config;
        }

        // POST api/<controller>
        [HttpPost]
        public GenericResponse Post([FromBody] string connectionString)
        {
            var response = new GenericResponse { Success = false, Message = "" };

            if (ModelState.IsValid)
            {
                bool exists = IsInstalled().Success;

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
                            response.Message = "Can Not Create Database - " + ex.Message;
                        }

                        // sleep to allow SQL server to attach new database
                        Thread.Sleep(5000);
                    }

                    if (exists)
                    {
                        // get master initialization script and update connectionstring and alias in seed data
                        string initializationScript = "";
                        using (StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\Scripts\\Master.sql"))
                        {
                            initializationScript = reader.ReadToEnd();
                        }
                        initializationScript = initializationScript.Replace("{ConnectionString}", connectionString);
                        initializationScript = initializationScript.Replace("{Alias}", HttpContext.Request.Host.Value);

                        var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
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
            string connectionString = _config.GetConnectionString("DefaultConnection");
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
                    response.Message = "Scripts Have Not Been Run";
                }
            }

            return response;
        }
    }
}

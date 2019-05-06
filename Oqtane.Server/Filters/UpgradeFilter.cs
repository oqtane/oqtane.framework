using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using DbUp;
using System.Data.SqlClient;
using System.Threading;
using System.IO;

namespace Oqtane.Filters
{
    public class UpgradeFilter : IStartupFilter
    {
        private readonly IConfiguration _config;

        public UpgradeFilter(IConfiguration config)
        {
            _config = config;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            string datadirectory = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string connectionString = _config.GetConnectionString("DefaultConnection");
            connectionString = connectionString.Replace("|DataDirectory|", datadirectory);

            // check if database exists
            SqlConnection connection = new SqlConnection(connectionString);
            bool databaseExists;
            try
            {
                using (connection)
                {
                    connection.Open();
                }
                databaseExists = true;
            }
            catch
            {
                databaseExists = false;
            }

            // create database if it does not exist
            if (!databaseExists)
            {
                string masterConnectionString = "";
                string databaseName = "";
                string[] fragments = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach(string fragment in fragments)
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
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                // sleep to allow SQL server to attach new database
                Thread.Sleep(5000);
            }

            // get initialization script and update connectionstring in Tenants seed data
            string initializationScript = "";
            using (StreamReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\Scripts\\Initialize.sql"))
            {
                initializationScript = reader.ReadToEnd();
            }
            initializationScript = initializationScript.Replace("{ConnectionString}", connectionString);

            // handle upgrade scripts
            var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                .WithScript(new DbUp.Engine.SqlScript("Initialize.sql", initializationScript))
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly()); // upgrade scripts should be added to /Scripts folder as Embedded Resources
            var dbUpgrade = dbUpgradeConfig.Build();
            if (dbUpgrade.IsUpgradeRequired())
            {
                var result = dbUpgrade.PerformUpgrade();
                if (!result.Successful)
                {
                    throw new Exception();
                }
            }
            return next;
        }
    }
}

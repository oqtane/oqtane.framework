using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using DbUp;
using System.Data.SqlClient;
using System.Threading;

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
                        if (connectionString.ToLower().Contains("attachdbfilename="))
                        {
                            command = new SqlCommand("CREATE DATABASE " + databaseName + " ON ( NAME = '" + databaseName + "', FILENAME = '" + datadirectory + "\\" + databaseName + ".mdf')", connection);
                        }
                        else
                        {
                            command = new SqlCommand("CREATE DATABASE " + databaseName, connection);
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

            // handle upgrade scripts
            var dbUpgradeConfig = DeployChanges.To.SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly());
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

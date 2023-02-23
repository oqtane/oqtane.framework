using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Oqtane.Databases.Interfaces;
using Oqtane.Models;
// ReSharper disable ConvertToUsingDeclaration
// ReSharper disable InvertIf
// ReSharper disable BuiltInTypeReferenceStyleForMemberAccess

namespace Oqtane.Repository
{
    public class SqlRepository : ISqlRepository
    {
        private IConfigurationRoot _config;

        public SqlRepository(IConfigurationRoot config)
        {
            _config = config;
        }

        public void ExecuteScript(Tenant tenant, string script)
        {
            // execute script in current tenant
            foreach (var query in script.Split("GO", StringSplitOptions.RemoveEmptyEntries))
            {
                ExecuteNonQuery(tenant, query);
            }
        }

        public bool ExecuteScript(string connectionString, string databaseType, Assembly assembly, string fileName)
        {
            var success = true;
            var script = GetScriptFromAssembly(assembly, fileName);

            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    foreach (var query in script.Split("GO", StringSplitOptions.RemoveEmptyEntries))
                    {
                        ExecuteNonQuery(connectionString, databaseType, query);
                    }
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        public bool ExecuteScript(Tenant tenant, Assembly assembly, string fileName)
        {
            var success = true;
            var script = GetScriptFromAssembly(assembly, fileName);

            if (!string.IsNullOrEmpty(script))
            {
                try
                {
                    ExecuteScript(tenant, script);
                }
                catch
                {
                    success = false;
                }
            }

            return success;
        }

        public int ExecuteNonQuery(Tenant tenant, string query)
        {
            return ExecuteNonQuery(tenant.DBConnectionString, tenant.DBType, query);
        }

        public IDataReader ExecuteReader(Tenant tenant, string query)
        {
            var db = GetActiveDatabase(tenant.DBType);
            return db.ExecuteReader(GetConnectionString(tenant.DBConnectionString), query);
        }

        public IDataReader ExecuteReader(string DBType, string DBConnectionString, string query)
        {
            var db = GetActiveDatabase(DBType);
            return db.ExecuteReader(GetConnectionString(DBConnectionString), query);
        }

        public int ExecuteNonQuery(string connectionString, string databaseType, string query)
        {
            var db = GetActiveDatabase(databaseType);
            return db.ExecuteNonQuery(GetConnectionString(connectionString), query);
        }

        public string GetScriptFromAssembly(Assembly assembly, string fileName)
        {
            // script must be included as an Embedded Resource within an assembly
            var script = "";

            if (assembly != null)
            {
                var name = assembly.GetManifestResourceNames().FirstOrDefault(item => item.EndsWith("." + fileName));
                if (name != null)
                {
                    var resourceStream = assembly.GetManifestResourceStream(name);
                    if (resourceStream != null)
                    {
                        using (var reader = new StreamReader(resourceStream))
                        {
                            script = reader.ReadToEnd();
                        }
                    }
                }
            }

            return script;
        }

        private IDatabase GetActiveDatabase(string databaseType)
        {
            IDatabase activeDatabase = null;
            if (!String.IsNullOrEmpty(databaseType))
            {
                var type = Type.GetType(databaseType);
                activeDatabase = Activator.CreateInstance(type) as IDatabase;
            }

            return activeDatabase;
        }

        private string GetConnectionString(string connectionString)
        {
            if (!connectionString.Contains("="))
            {
                connectionString = _config.GetConnectionString(connectionString);
            }
            return connectionString;
        }
    }
}

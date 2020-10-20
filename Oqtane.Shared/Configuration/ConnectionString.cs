using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oqtane.Configuration
{
    public static class ConnectionString
    {
        public static readonly string DataDirectoryVariable = $@"|{DataDirectory.KeyName}|";

        public static string Normalize(string connectionString)
        {
            var ret = connectionString.Replace(DataDirectoryVariable, DataDirectory.Current());
            return connectionString;
        }

        public static string Denormalize(string connectionString)
        {
            var ret = connectionString.Replace(DataDirectory.Current(), DataDirectoryVariable);
            return ret;
        }
    }
}

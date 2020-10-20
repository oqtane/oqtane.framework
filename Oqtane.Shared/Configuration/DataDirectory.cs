#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oqtane.Configuration
{
    public static class DataDirectory
    {
        public static readonly string KeyName = "DataDirectory";

        public static string Current()
        {
            var ret = AppContext.GetData(KeyName)?.ToString() ?? String.Empty;
            return ret;
        }

        public static void Set(string Value)
        {
            AppDomain.CurrentDomain.SetData(KeyName, Value);
        }

    }

}

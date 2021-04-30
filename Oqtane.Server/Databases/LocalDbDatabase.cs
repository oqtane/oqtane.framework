using System;
using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Repository.Databases;

namespace Oqtane.Databases
{
    public class LocalDbDatabase : SqlServerDatabaseBase
    {
        private static string _friendlyName => "Local Database";
        private static string _name => "LocalDB";

        public LocalDbDatabase() :base(_name, _friendlyName) { }
    }
}

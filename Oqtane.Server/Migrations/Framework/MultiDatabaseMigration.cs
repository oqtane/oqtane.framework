using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Interfaces;

namespace Oqtane.Migrations
{
    public abstract class MultiDatabaseMigration : Migration
    {
        private readonly IEnumerable<IOqtaneDatabase> _databases;

        protected MultiDatabaseMigration(IEnumerable<IOqtaneDatabase> databases)
        {
            _databases = databases;
        }

        protected IOqtaneDatabase ActiveDatabase => _databases.FirstOrDefault(d => d.Provider == ActiveProvider);

        protected string RewriteName(string name)
        {
            return ActiveDatabase.RewriteName(name);
        }
    }
}

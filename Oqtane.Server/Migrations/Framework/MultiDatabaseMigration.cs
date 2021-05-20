using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

namespace Oqtane.Migrations
{
    public abstract class MultiDatabaseMigration : Migration
    {
        protected MultiDatabaseMigration(IOqtaneDatabase database)
        {
            ActiveDatabase = database;
        }

        protected IOqtaneDatabase ActiveDatabase { get; }

        protected string RewriteName(string name)
        {
            return ActiveDatabase.RewriteName(name);
        }
    }
}

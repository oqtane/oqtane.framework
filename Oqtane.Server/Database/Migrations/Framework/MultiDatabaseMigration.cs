using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Interfaces;

namespace Oqtane.Database.Migrations.Framework
{
    public abstract class MultiDatabaseMigration : Migration
    {
        private readonly IOqtaneDatabase _databases;

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

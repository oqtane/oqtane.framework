using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;

namespace Oqtane.Migrations
{
    public abstract class MultiDatabaseMigration : Migration
    {
        protected MultiDatabaseMigration(IDatabase database)
        {
            ActiveDatabase = database;
        }

        protected IDatabase ActiveDatabase { get; }

        protected string RewriteName(string name)
        {
            return ActiveDatabase.RewriteName(name);
        }
    }
}

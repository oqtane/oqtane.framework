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

        protected string DelimitName(string name)
        {
            return ActiveDatabase.DelimitName(name);
        }

        protected string RewriteName(string name)
        {
            return ActiveDatabase.RewriteName(name);
        }

        protected string RewriteValue(object value)
        {
            return ActiveDatabase.RewriteValue(value);
        }
    }
}

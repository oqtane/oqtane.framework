using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.05.02.04.01")]
    public class RemoveLanguageName : MultiDatabaseMigration
    {
        public RemoveLanguageName(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name != "Sqlite")
            {
                var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder, ActiveDatabase);
                languageEntityBuilder.DropColumn("Name");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

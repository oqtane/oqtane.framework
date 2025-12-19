using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.06.00.01.01")]
    public class AddLanguageName : MultiDatabaseMigration
    {
        public AddLanguageName(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Name column was removed in 5.2.4 however SQLite did not support column removal so it had to be restored
            if (ActiveDatabase.Name != "Sqlite")
            {
                var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder, ActiveDatabase);
                languageEntityBuilder.AddStringColumn("Name", 100, true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

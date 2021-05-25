using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.02")]
    public class AddLanguageTable : MultiDatabaseMigration
    {
        public AddLanguageTable(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Language table
            var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder, ActiveDatabase);
            languageEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Language table
            var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder, ActiveDatabase);
            languageEntityBuilder.Drop();
        }
    }
}

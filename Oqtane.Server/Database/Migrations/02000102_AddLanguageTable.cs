using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Database.Migrations.Framework;
using Oqtane.Interfaces;
using Oqtane.Database.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Database.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.02")]
    public class AddLanguageTable : MultiDatabaseMigration
    {
        public AddLanguageTable(IOqtaneDatabase database) : base(database)
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

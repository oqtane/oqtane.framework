using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.02")]
    public class AddLanguageTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Language table
            var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder);
            languageEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Language table
            var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder);
            languageEntityBuilder.Drop();
        }
    }
}

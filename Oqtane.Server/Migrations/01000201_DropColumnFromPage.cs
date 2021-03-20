using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.02.01")]
    public class DropColumnFromPage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Page table
            if (migrationBuilder.ActiveProvider != "Microsoft.EntityFrameworkCore.Sqlite")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder);
                pageEntityBuilder.DropColumn("EditMode");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Add Column to Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder);
            pageEntityBuilder.AddBooleanColumn("EditMode");
        }
    }
}

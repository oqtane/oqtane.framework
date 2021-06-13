using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.02.01")]
    public class DropColumnFromPage : MultiDatabaseMigration
    {
        public DropColumnFromPage(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Page table
            if (ActiveDatabase.Name == "SqlServer")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.DropColumn("EditMode");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Add Column to Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            pageEntityBuilder.AddBooleanColumn("EditMode");
        }
    }
}

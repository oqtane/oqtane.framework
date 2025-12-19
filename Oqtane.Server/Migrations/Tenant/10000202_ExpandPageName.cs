using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.00.02.02")]
    public class ExpandPageName : MultiDatabaseMigration
    {
        public ExpandPageName(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            pageEntityBuilder.AlterStringColumn("Name", 100);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

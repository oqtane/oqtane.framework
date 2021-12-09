using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.01.02")]
    public class AddVisitorTable : MultiDatabaseMigration
    {
        public AddVisitorTable(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
            visitorEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
            visitorEntityBuilder.Drop();
        }
    }
}

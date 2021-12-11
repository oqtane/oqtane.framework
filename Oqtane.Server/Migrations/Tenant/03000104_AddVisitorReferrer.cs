using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.01.05")]
    public class AddVisitorReferrer : MultiDatabaseMigration
    {
        public AddVisitorReferrer(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);

            visitorEntityBuilder.AddStringColumn("Referrer", 500, true);
            visitorEntityBuilder.AddStringColumn("Url", 500, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);

            visitorEntityBuilder.DropColumn("Referrer");
            visitorEntityBuilder.DropColumn("Url");
        }
    }
}

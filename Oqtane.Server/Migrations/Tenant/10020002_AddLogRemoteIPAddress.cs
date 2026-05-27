using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.02.00.02")]
    public class AddLogRemoteIPAddress : MultiDatabaseMigration
    {
        public AddLogRemoteIPAddress(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var logEntityBuilder = new LogEntityBuilder(migrationBuilder, ActiveDatabase);
            logEntityBuilder.AddStringColumn("RemoteIPAddress", 50, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

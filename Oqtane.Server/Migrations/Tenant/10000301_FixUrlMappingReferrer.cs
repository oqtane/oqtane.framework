using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.00.03.01")]
    public class FixUrlMappingReferrer : MultiDatabaseMigration
    {
        public FixUrlMappingReferrer(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
            urlMappingEntityBuilder.DropColumn("Referrer");
            urlMappingEntityBuilder.AddStringColumn("Referrer", 2048, true); // must be nullable
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

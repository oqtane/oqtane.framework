using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.00.02.03")]
    public class AddUrlMappingReferrer : MultiDatabaseMigration
    {
        public AddUrlMappingReferrer(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
            urlMappingEntityBuilder.AddStringColumn("Referrer", 2048);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

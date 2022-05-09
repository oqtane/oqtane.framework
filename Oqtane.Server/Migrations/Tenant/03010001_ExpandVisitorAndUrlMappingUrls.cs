using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.01.00.01")]
    public class ExpandVisitorAndUrlMappingUrls : MultiDatabaseMigration
    {
        public ExpandVisitorAndUrlMappingUrls(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
            visitorEntityBuilder.AlterStringColumn("Url", 2048);

            var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
            urlMappingEntityBuilder.AlterStringColumn("MappedUrl", 2048);
            // Url is an index column and MySQL only supports indexes of 3072 bytes (this index will be 750X4+4=3004 bytes)
            urlMappingEntityBuilder.AlterStringColumn("Url", 750, false, true, "IX_UrlMapping:SiteId,Url:true");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
            visitorEntityBuilder.AlterStringColumn("Url", 500);

            var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
            urlMappingEntityBuilder.AlterStringColumn("MappedUrl", 500);
            urlMappingEntityBuilder.AlterStringColumn("Url", 500, false, true, "IX_UrlMapping:SiteId,Url:true");
        }
    }
}

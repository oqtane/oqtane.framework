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
            // Drop the index is needed because the Url is already associated with IX_UrlMapping
            urlMappingEntityBuilder.DropIndex("IX_UrlMapping");
            urlMappingEntityBuilder.AlterStringColumn("Url", 2048);
            urlMappingEntityBuilder.AlterStringColumn("MappedUrl", 2048);
            urlMappingEntityBuilder.AddIndex("IX_UrlMapping", new[] { "SiteId", "Url" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
            visitorEntityBuilder.AlterStringColumn("Url", 500);

            var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
            // Drop the index is needed because the Url is already associated with IX_UrlMapping
            urlMappingEntityBuilder.DropIndex("IX_UrlMapping");
            urlMappingEntityBuilder.AlterStringColumn("Url", 500);
            urlMappingEntityBuilder.AlterStringColumn("MappedUrl", 500);
            urlMappingEntityBuilder.AddIndex("IX_UrlMapping", new[] { "SiteId", "Url" }, true);
        }
    }
}

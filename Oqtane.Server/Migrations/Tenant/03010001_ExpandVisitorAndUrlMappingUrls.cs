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
            if (ActiveDatabase.Name != "Sqlite")
            {
                var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
                visitorEntityBuilder.AlterStringColumn("Url", 2048);

                // Drop the index is needed because the Url is already associated with IX_UrlMapping
                var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
                urlMappingEntityBuilder.DropForeignKey("FK_UrlMapping_Site");
                urlMappingEntityBuilder.DropIndex("IX_UrlMapping");
                urlMappingEntityBuilder.AlterStringColumn("Url", 2048);
                urlMappingEntityBuilder.AlterStringColumn("MappedUrl", 2048);
                urlMappingEntityBuilder.AddIndex("IX_UrlMapping", new[] { "SiteId", "Url" }, true);
                urlMappingEntityBuilder.AddForeignKey("FK_UrlMapping_Site");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name != "Sqlite")
            {
                var visitorEntityBuilder = new VisitorEntityBuilder(migrationBuilder, ActiveDatabase);
                visitorEntityBuilder.AlterStringColumn("Url", 500);

                var urlMappingEntityBuilder = new UrlMappingEntityBuilder(migrationBuilder, ActiveDatabase);
                urlMappingEntityBuilder.DropForeignKey("FK_UrlMapping_Site");
                urlMappingEntityBuilder.DropIndex("IX_UrlMapping");
                urlMappingEntityBuilder.AlterStringColumn("Url", 500);
                urlMappingEntityBuilder.AlterStringColumn("MappedUrl", 500);
                urlMappingEntityBuilder.AddIndex("IX_UrlMapping", new[] { "SiteId", "Url" }, true);
                urlMappingEntityBuilder.AddForeignKey("FK_UrlMapping_Site");
            }
        }
    }
}

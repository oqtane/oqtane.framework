using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.05.02.01.01")]
    public class AddIndexes : MultiDatabaseMigration
    {
        public AddIndexes(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder, ActiveDatabase);
            languageEntityBuilder.AddIndex("IX_Language", new string[] { "SiteId", "Code" }, true);

            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            pageModuleEntityBuilder.AddIndex("IX_PageModule_Module", "ModuleId");
            pageModuleEntityBuilder.AddIndex("IX_PageModule_Page", "PageId");

            var searchContentEntityBuilder = new SearchContentEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentEntityBuilder.AddIndex("IX_SearchContent", new string[] { "SiteId", "EntityName", "EntityId" }, true);

            var searchContentPropertyEntityBuilder = new SearchContentPropertyEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentPropertyEntityBuilder.AddIndex("IX_SearchContentProperty", new string[] { "SearchContentId", "Name" }, true);

            var searchContentWordEntityBuilder = new SearchContentWordEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordEntityBuilder.AddIndex("IX_SearchContentWord", new string[] { "SearchContentId", "SearchWordId" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

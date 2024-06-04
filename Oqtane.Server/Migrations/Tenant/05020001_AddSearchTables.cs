using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.05.02.00.01")]
    public class AddSearchTables : MultiDatabaseMigration
    {
        public AddSearchTables(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var searchContentEntityBuilder = new SearchContentEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentEntityBuilder.Create();

            var searchContentPropertyEntityBuilder = new SearchContentPropertyEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentPropertyEntityBuilder.Create();

            var searchContentWordSourceEntityBuilder = new SearchContentWordSourceEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordSourceEntityBuilder.Create();
            searchContentWordSourceEntityBuilder.AddIndex("IX_SearchContentWordSource", "Word", true);

            var searchContentWordsEntityBuilder = new SearchContentWordsEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordsEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var searchContentWordsEntityBuilder = new SearchContentWordsEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordsEntityBuilder.Drop();

            var searchContentWordSourceEntityBuilder = new SearchContentWordSourceEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordSourceEntityBuilder.DropIndex("IX_SearchContentWordSource");
            searchContentWordSourceEntityBuilder.Drop();

            var searchContentPropertyEntityBuilder = new SearchContentPropertyEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentPropertyEntityBuilder.Drop();

            var searchContentEntityBuilder = new SearchContentEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentEntityBuilder.Drop();
        }
    }
}

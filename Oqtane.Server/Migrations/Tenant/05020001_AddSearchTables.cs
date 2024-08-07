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

            var searchWordEntityBuilder = new SearchWordEntityBuilder(migrationBuilder, ActiveDatabase);
            searchWordEntityBuilder.Create();
            searchWordEntityBuilder.AddIndex("IX_SearchWord", "Word", true);

            var searchContentWordEntityBuilder = new SearchContentWordEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var searchContentWordEntityBuilder = new SearchContentWordEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentWordEntityBuilder.Drop();

            var searchWordEntityBuilder = new SearchWordEntityBuilder(migrationBuilder, ActiveDatabase);
            searchWordEntityBuilder.DropIndex("IX_SearchWord");
            searchWordEntityBuilder.Drop();

            var searchContentPropertyEntityBuilder = new SearchContentPropertyEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentPropertyEntityBuilder.Drop();

            var searchContentEntityBuilder = new SearchContentEntityBuilder(migrationBuilder, ActiveDatabase);
            searchContentEntityBuilder.Drop();
        }
    }
}

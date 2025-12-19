using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.10.00.02.01")]
    public class RemoveDeprecatedColumns : MultiDatabaseMigration
    {
        public RemoveDeprecatedColumns(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Oqtane 10.0.2 includes support for column removal in SQLite, so we can now clean up deprecated columns

            // Folder columns were deprecated in Oqtane 3.2.2 
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.DropColumn("IsDeleted"); 
            if (ActiveDatabase.Name == "Sqlite")
            {
                /// the following columns were not added back in 3.2.3 but they still exist in SQLite databases
                folderEntityBuilder.DropColumn("DeletedBy");
                folderEntityBuilder.DropColumn("DeletedOn");
            }

            // File columns were deprecated in Oqtane 3.2.2 
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            // IsDeleted was added back in 3.2.3 for non-SQLLite databases
            fileEntityBuilder.DropColumn("IsDeleted");
            if (ActiveDatabase.Name == "Sqlite")
            {
                /// the following columns were not added back in 3.2.3 but they still exist in SQLite databases
                fileEntityBuilder.DropColumn("DeletedBy");
                fileEntityBuilder.DropColumn("DeletedOn");
            }

            // Language columns were deprecated in Oqtane 5.2.4 
            var languageEntityBuilder = new LanguageEntityBuilder(migrationBuilder, ActiveDatabase);
            languageEntityBuilder.DropColumn("Name");

            // Site columns were deprecated in Oqtane 10.0.1 
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            if (ActiveDatabase.Name == "Sqlite")
            {
                /// the following column was removed for non-SQLite databases in 10.0.1
                siteEntityBuilder.DropColumn("TenantId");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

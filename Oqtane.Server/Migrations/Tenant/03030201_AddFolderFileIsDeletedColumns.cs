using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.03.02.01")]
    public class AddFolderFileIsDeletedColumns : MultiDatabaseMigration
    {
        public AddFolderFileIsDeletedColumns(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IsDeleted columns were removed in 3.2.2 however SQLite does not support column removal so they had to be restored
            if (ActiveDatabase.Name != "Sqlite")
            {
                var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
                folderEntityBuilder.AddBooleanColumn("IsDeleted", true);

                var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
                fileEntityBuilder.AddBooleanColumn("IsDeleted", true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

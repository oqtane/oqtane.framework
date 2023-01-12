using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.02.02.01")]
    public class RemoveFolderFileDeletableColumns : MultiDatabaseMigration
    {
        public RemoveFolderFileDeletableColumns(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.DropColumn("DeletedBy");
            folderEntityBuilder.DropColumn("DeletedOn");
            folderEntityBuilder.DropColumn("IsDeleted");

            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.DropColumn("DeletedBy");
            fileEntityBuilder.DropColumn("DeletedOn");
            fileEntityBuilder.DropColumn("IsDeleted");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.03.00.01.01")]
    public class ChangeFileNameColumnsSize : MultiDatabaseMigration
    {
        public ChangeFileNameColumnsSize(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name != "Sqlite")
            {
                var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);

                // Drop the index is needed because the Name is already associated with IX_File
                fileEntityBuilder.DropForeignKey("FK_File_Folder");
                fileEntityBuilder.DropIndex("IX_File");
                fileEntityBuilder.AlterStringColumn("Name", 256);
                fileEntityBuilder.AddIndex("IX_File", new[] { "FolderId", "Name" }, true);
                fileEntityBuilder.AddForeignKey("FK_File_Folder");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name != "Sqlite")
            {
                var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);

                fileEntityBuilder.DropIndex("IX_File");
                fileEntityBuilder.AlterStringColumn("Name", 50);
                fileEntityBuilder.AddIndex("IX_File", new[] { "FolderId", "Name" }, true);
            }
        }
    }
}

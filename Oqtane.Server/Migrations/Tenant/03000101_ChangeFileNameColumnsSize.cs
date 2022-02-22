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
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            // Drop the index is needed because the Name is already associated with IX_File
            fileEntityBuilder.DropIndex("IX_File");
            fileEntityBuilder.AlterStringColumn("Name", 256);
            fileEntityBuilder.AddIndex("IX_File", new[] { "FolderId", "Name" }, true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            // Drop the index is needed because the Name is already associated with IX_File
            fileEntityBuilder.DropIndex("IX_File");
            fileEntityBuilder.AlterStringColumn("Name", 50);
            fileEntityBuilder.AddIndex("IX_File", new[] { "FolderId", "Name" }, true);
        }
    }
}

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
            fileEntityBuilder.AlterStringColumn("Name", 256, false, true, "IX_File:FolderId,Name:true");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.AlterStringColumn("Name", 50, false, true, "IX_File:FolderId,Name:true");
        }
    }
}

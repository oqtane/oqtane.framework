using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.01.00.03")]
    public class AddFolderType : MultiDatabaseMigration
    {
        public AddFolderType(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);

            //Add Type column and initialize
            folderEntityBuilder.AddStringColumn("Type", 50, true, true);
            folderEntityBuilder.UpdateColumn("Type", "'" + FolderTypes.Private + "'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);

            folderEntityBuilder.DropColumn("Type");
        }
    }
}

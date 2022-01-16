using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.03.00.02.01")]
    public class AddAliasIsDefault : MultiDatabaseMigration
    {
        public AddAliasIsDefault(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.AddBooleanColumn("IsDefault", true);
            aliasEntityBuilder.UpdateColumn("IsDefault", "1", "bool", "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.DropColumn("IsDefault");
        }
    }
}

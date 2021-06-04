using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.02.01.00.01")]
    public class AddDatabaseTypeColumnToTenant : MultiDatabaseMigration
    {
        public AddDatabaseTypeColumnToTenant(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Site table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.AddStringColumn("DBType", 200, true);

            //Update new column if SqlServer (Other Databases will not have any records yet)
            if (ActiveDatabase.Name == "SqlServer")
            {
                tenantEntityBuilder.UpdateColumn("DBType", $"'{ActiveDatabase.TypeName}'");
            }
        }
    }
}

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.02.01.00.01")]
    public class AddDatabaseTypeColumnToTenant : MultiDatabaseMigration
    {
        public AddDatabaseTypeColumnToTenant(IOqtaneDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Site table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.AddStringColumn("DBType", 200, true);

            //Update new column if SqlServer (Other Databases will not have any records yet)
            if (ActiveDatabase.Name == "SqlServer" || ActiveDatabase.Name == "LocalDB")
            {
                tenantEntityBuilder.UpdateColumn("DBType", "'SqlServer'");
            }
        }
    }
}

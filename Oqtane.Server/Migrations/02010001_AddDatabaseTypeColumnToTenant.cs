using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.02.01.00.01")]
    public class AddDatabaseTypeColumnToTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Add Column to Site table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder);
            tenantEntityBuilder.AddStringColumn("DBType", 200, true);

            //Update new column
            migrationBuilder.Sql(
                @"
                    UPDATE Tenant
                    SET DBType = 'Oqtane.Repository.Databases.SqlServerDatabase, Oqtane.Server'
                ");

        }
    }
}

using System.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.06.01.05.01")]
    public class UpdateTenantDBType : MultiDatabaseMigration
    {
        public UpdateTenantDBType(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // database providers moved to oqtane.server
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.UpdateData("DBType", "'Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Server'", $"{DelimitName(RewriteName("DBType"))} = 'Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer'");
            tenantEntityBuilder.UpdateData("DBType", "'Oqtane.Database.Sqlite.SqliteDatabase, Oqtane.Server'", $"{DelimitName(RewriteName("DBType"))} = 'Oqtane.Database.Sqlite.SqliteDatabase, Oqtane.Database.Sqlite'");
            tenantEntityBuilder.UpdateData("DBType", "'Oqtane.Database.MySQL.MySQLDatabase, Oqtane.Server'", $"{DelimitName(RewriteName("DBType"))} = 'Oqtane.Database.MySQL.MySQLDatabase, Oqtane.Database.MySQL'");
            tenantEntityBuilder.UpdateData("DBType", "'Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Server'", $"{DelimitName(RewriteName("DBType"))} = 'Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Database.PostgreSQL'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

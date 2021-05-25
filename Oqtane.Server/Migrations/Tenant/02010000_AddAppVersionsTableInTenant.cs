using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.01.00.00")]
    public class AddAppVersionsTable : MultiDatabaseMigration
    {
        public AddAppVersionsTable(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create AppVersions table
            var appVersionsEntityBuilder = new AppVersionsEntityBuilder(migrationBuilder, ActiveDatabase);
            appVersionsEntityBuilder.Create();

            //Finish SqlServer Migration from DbUp
            if (ActiveDatabase.Name == "SqlServer")
            {
                //Version 1.0.0
                InsertVersion(migrationBuilder, "01.00.00", "Oqtane.Scripts.Master.00.09.00.00.sql");

                //Version 1.0.1
                InsertVersion(migrationBuilder, "01.00.01", "Oqtane.Scripts.Master.01.00.01.00.sql");

                //Version 1.0.2
                InsertVersion(migrationBuilder, "01.00.02", "Oqtane.Scripts.Tenant.01.00.02.01.sql");

                //Version 2.0.0
                InsertVersion(migrationBuilder, "02.00.00", "Oqtane.Scripts.Tenant.02.00.00.01.sql");

                //Version 2.0.1
                InsertVersion(migrationBuilder, "02.00.01", "Oqtane.Scripts.Tenant.02.00.01.03.sql");

                //Version 2.0.2
                InsertVersion(migrationBuilder, "02.00.02", "Oqtane.Scripts.Tenant.02.00.02.01.sql");

                //Drop SchemaVersions
                migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.SchemaVersions') AND OBJECTPROPERTY(id, N'IsTable') = 1)
                    BEGIN
                        DROP TABLE SchemaVersions
                    END");
            }

            //Version 2.1.0
            var appVersions = RewriteName("AppVersions");
            var version = RewriteName("Version");
            var appledDate = RewriteName("AppliedDate");

            migrationBuilder.Sql($@"
                INSERT INTO {appVersions}({version}, {appledDate})
                    VALUES('02.01.00', '{DateTime.UtcNow.ToString("yyyy'-'MM'-'dd HH':'mm':'ss")}')
            ");
        }

        private void InsertVersion(MigrationBuilder migrationBuilder, string version, string scriptName)
        {
            migrationBuilder.Sql($@"
                IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.SchemaVersions') AND OBJECTPROPERTY(id, N'IsTable') = 1)
                    BEGIN
                        INSERT INTO AppVersions(Version, AppliedDate)
                        SELECT Version = '{version}', Applied As AppliedDate
                            FROM SchemaVersions
                            WHERE ScriptName = '{scriptName}'
                    END
            ");
        }
    }
}

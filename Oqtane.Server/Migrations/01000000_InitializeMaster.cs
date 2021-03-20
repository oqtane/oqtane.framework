using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Migrations.Extensions;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.01.00.00.00")]
    public class InitializeMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder);
            tenantEntityBuilder.Create();

            //Create Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder);
            aliasEntityBuilder.Create();

            //Create ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder);
            moduleDefinitionsEntityBuilder.Create();

            //Create Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder);
            jobEntityBuilder.Create();

            //Create JobLog Table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder);
            jobLogEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder);
            aliasEntityBuilder.Drop();

            //Drop JobLog Table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder);
            jobLogEntityBuilder.Drop();

            //Drop Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder);
            tenantEntityBuilder.Drop();

            //Drop ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder);
            moduleDefinitionsEntityBuilder.Drop();

            //Drop Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder);
            jobEntityBuilder.Drop();
        }
    }
}

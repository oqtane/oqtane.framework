using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.01.00.00.00")]
    public class InitializeMaster : MultiDatabaseMigration
    {
        public InitializeMaster(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.Create();

            //Create Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.Create();

            //Create ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleDefinitionsEntityBuilder.Create();

            //Create Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
            jobEntityBuilder.Create();

            //Create JobLog Table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder, ActiveDatabase);
            jobLogEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop Alias table
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.Drop();

            //Drop JobLog Table
            var jobLogEntityBuilder = new JobLogEntityBuilder(migrationBuilder, ActiveDatabase);
            jobLogEntityBuilder.Drop();

            //Drop Tenant table
            var tenantEntityBuilder = new TenantEntityBuilder(migrationBuilder, ActiveDatabase);
            tenantEntityBuilder.Drop();

            //Drop ModuleDefinitions Table
            var moduleDefinitionsEntityBuilder = new ModuleDefinitionsEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleDefinitionsEntityBuilder.Drop();

            //Drop Job Table
            var jobEntityBuilder = new JobEntityBuilder(migrationBuilder, ActiveDatabase);
            jobEntityBuilder.Drop();
        }

    }
}

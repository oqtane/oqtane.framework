using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Application.Migrations.EntityBuilders;
using Oqtane.Application.Repository;

namespace Oqtane.Application.Migrations
{
    [DbContext(typeof(Context))]
    [Migration("Oqtane.Application.01.00.00.00")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var myModuleEntityBuilder = new MyModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            myModuleEntityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var myModuleEntityBuilder = new MyModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            myModuleEntityBuilder.Drop();
        }
    }
}

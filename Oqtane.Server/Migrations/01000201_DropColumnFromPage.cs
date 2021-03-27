using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.02.01")]
    public class DropColumnFromPage : MultiDatabaseMigration
    {
        public DropColumnFromPage(IEnumerable<IOqtaneDatabase> databases) : base(databases)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Drop Column from Page table
            if (ActiveDatabase.Name == "SqlServer" || ActiveDatabase.Name == "LocalDB")
            {
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.DropColumn("EditMode");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Add Column to Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            pageEntityBuilder.AddBooleanColumn("EditMode");
        }
    }
}

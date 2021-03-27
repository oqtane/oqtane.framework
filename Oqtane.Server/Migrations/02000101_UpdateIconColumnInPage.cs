using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Interfaces;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.01")]
    public class UpdateIconColumnInPage : MultiDatabaseMigration
    {
        public UpdateIconColumnInPage(IEnumerable<IOqtaneDatabase> databases) : base(databases)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ///Update Icon Field in Page
            migrationBuilder.Sql(
                @"
                    UPDATE Page
                    SET Icon = 'oi oi-' + Icon
                    WHERE Icon <> ''
                ");
        }
    }
}

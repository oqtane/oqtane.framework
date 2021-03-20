using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.01")]
    public class UpdateIconColumnInPage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ///Update Icon Field in Page
            migrationBuilder.Sql(
                @"
                    UPDATE [Page]
                    SET Icon = IIF(Icon <> '', 'oi oi-' + Icon, '');
                ");
        }
    }
}

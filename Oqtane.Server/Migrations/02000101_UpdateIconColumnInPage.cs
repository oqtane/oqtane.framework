using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.01.01")]
    public class UpdateIconColumnInPage : MultiDatabaseMigration
    {
        public UpdateIconColumnInPage(IOqtaneDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            ///Update Icon Field in Page
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            var updateSql = ActiveDatabase.ConcatenateSql("'oi oi-'", $"{ActiveDatabase.RewriteName("Icon")}");
            pageEntityBuilder.UpdateColumn("Icon", updateSql, $"{ActiveDatabase.RewriteName("Icon")} <> ''" );
        }
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Interfaces;
using Oqtane.Modules.HtmlText.Migrations.EntityBuilders;
using Oqtane.Modules.HtmlText.Repository;
using Oqtane.Database.Migrations.Framework;

namespace Oqtane.Modules.HtmlText.Migrations
{
    [DbContext(typeof(HtmlTextContext))]
    [Migration("HtmlText.01.00.00.00")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IOqtaneDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create HtmlText table
            var entityBuilder = new HtmlTextEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop HtmlText table
            var entityBuilder = new HtmlTextEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();
        }
    }
}

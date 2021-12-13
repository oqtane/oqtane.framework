using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Documentation;
using Oqtane.Migrations;
using Oqtane.Modules.HtmlText.Migrations.EntityBuilders;
using Oqtane.Modules.HtmlText.Repository;

namespace Oqtane.Modules.HtmlText.Migrations
{
    [DbContext(typeof(HtmlTextContext))]
    [Migration("HtmlText.01.00.00.00")]
    [PrivateApi("Mark HtmlText classes as private, since it's not very useful in the public docs")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new HtmlTextEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new HtmlTextEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();
        }
    }
}

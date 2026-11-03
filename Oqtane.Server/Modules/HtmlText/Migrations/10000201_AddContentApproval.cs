using Microsoft.AspNetCore.Http.HttpResults;
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
    [Migration("HtmlText.01.00.02.00")]
    public class AddContentApproval : MultiDatabaseMigration
    {
        public AddContentApproval(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var htmlTextEntityBuilder = new HtmlTextEntityBuilder(migrationBuilder, ActiveDatabase);
            htmlTextEntityBuilder.AddIntegerColumn("State", true);
            htmlTextEntityBuilder.AddStringColumn("Comment", 1000, true);
            htmlTextEntityBuilder.UpdateData("State", 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

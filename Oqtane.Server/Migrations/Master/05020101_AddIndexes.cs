using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Master
{
    [DbContext(typeof(MasterDBContext))]
    [Migration("Master.05.02.01.01")]
    public class AddIndexes : MultiDatabaseMigration
    {
        public AddIndexes(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var aliasEntityBuilder = new AliasEntityBuilder(migrationBuilder, ActiveDatabase);
            aliasEntityBuilder.DropIndex("IX_Alias");
            aliasEntityBuilder.AddIndex("IX_Alias", "Name", true);

            var themeEntityBuilder = new ThemeEntityBuilder(migrationBuilder, ActiveDatabase);
            themeEntityBuilder.AddIndex("IX_Theme", "ThemeName", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // not implemented
        }
    }
}

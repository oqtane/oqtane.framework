using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.02.03")]
    public class DropDefaultLayoutInSite : MultiDatabaseMigration
    {
        public DropDefaultLayoutInSite(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name == "SqlServer")
            {
                //Alter Column in Setting table for Sql Server
                var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
                settingEntityBuilder.DropIndex("IX_Setting");
                settingEntityBuilder.AlterStringColumn("SettingName", 200);
                settingEntityBuilder.AddIndex("IX_Setting", new [] {"EntityName", "EntityId", "SettingName"}, true);

                //Drop Column from Site Table
                var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
                siteEntityBuilder.DropColumn("DefaultLayoutType");

                //Update DefaultContainerType In Site
                siteEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'");
                siteEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'");

                //Drop Column from Page Table
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.DropColumn("LayoutType");

                //Update DefaultContainerType in Page
                pageEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'");
                pageEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'");


                //Update ContainerType in PageModule
                var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
                pageModuleEntityBuilder.UpdateColumn("ContainerType", "'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'", "ContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'");
                pageModuleEntityBuilder.UpdateColumn("ContainerType", "'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'", "ContainerType = 'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'");

            }
        }
    }
}

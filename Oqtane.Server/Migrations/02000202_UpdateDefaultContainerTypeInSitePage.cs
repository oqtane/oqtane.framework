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
    [Migration("Tenant.02.00.02.02")]
    public class UpdateDefaultContainerTypeInSitePage : MultiDatabaseMigration
    {
        public UpdateDefaultContainerTypeInSitePage(IOqtaneDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name == "SqlServer" || ActiveDatabase.Name == "LocalDB")
            {
                //Update DefaultContainerType In Site
                var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
                siteEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'");
                siteEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client'");

                //Update DefaultContainerType in Page
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'");
                pageEntityBuilder.UpdateColumn("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'", "DefaultContainerType = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client'");

                //Update ContainerType in PageModule
                var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
                pageModuleEntityBuilder.UpdateColumn("ContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'", "ContainerType = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'");
                pageModuleEntityBuilder.UpdateColumn("ContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'", "ContainerType = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client'");
            }

        }
    }
}

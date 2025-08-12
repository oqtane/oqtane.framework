using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.02.00.02.02")]
    public class UpdateDefaultContainerTypeInSitePage : MultiDatabaseMigration
    {
        public UpdateDefaultContainerTypeInSitePage(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveDatabase.Name == "SqlServer")
            {
                //Update DefaultContainerType In Site
                var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
                siteEntityBuilder.UpdateData("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'", $"{DelimitName(RewriteName("DefaultContainerType"))} = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'");
                siteEntityBuilder.UpdateData("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'", $"{DelimitName(RewriteName("DefaultContainerType"))} = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client'");

                //Update DefaultContainerType in Page
                var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
                pageEntityBuilder.UpdateData("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'", $"{DelimitName(RewriteName("DefaultContainerType"))} = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'");
                pageEntityBuilder.UpdateData("DefaultContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'", $"{DelimitName(RewriteName("DefaultContainerType"))} = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client'");

                //Update ContainerType in PageModule
                var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
                pageModuleEntityBuilder.UpdateData("ContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultTitle, Oqtane.Client'", $"{DelimitName(RewriteName("ContainerType"))} = 'Oqtane.Themes.OqtaneTheme.Container, Oqtane.Client'");
                pageModuleEntityBuilder.UpdateData("ContainerType", "'Oqtane.Themes.OqtaneTheme.DefaultNoTitle, Oqtane.Client'", $"{DelimitName(RewriteName("ContainerType"))} = 'Oqtane.Themes.OqtaneTheme.NoTitle, Oqtane.Client'");
            }

        }
    }
}

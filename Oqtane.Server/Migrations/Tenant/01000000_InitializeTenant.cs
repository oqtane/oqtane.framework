using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;

namespace Oqtane.Migrations.Tenant
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.00.00")]
    public class InitializeTenant : MultiDatabaseMigration
    {
        public InitializeTenant(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.Create();

            //Add Column to Site table (for Sql Server only) we will drop it later for Sql Server only
            if (ActiveDatabase.Name == "SqlServer")
            {
                siteEntityBuilder.AddStringColumn("DefaultLayoutType", 200, true);
            }

            //Create Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            pageEntityBuilder.Create();
            pageEntityBuilder.AddIndex("IX_Page", new [] {"SiteId", "Path", "UserId"}, true);

            //Add Columns to Page table (for Sql Server only) we will drop them later for Sql Server only
            if (ActiveDatabase.Name == "SqlServer")
            {
                pageEntityBuilder.AddBooleanColumn("EditMode");
                pageEntityBuilder.AddStringColumn("LayoutType", 200, true);
            }

            //Create Module table
            var moduleEntityBuilder = new ModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleEntityBuilder.Create();

            //Create PageModule table
            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            pageModuleEntityBuilder.Create();

            //Create User table
            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.Create();
            userEntityBuilder.AddIndex("IX_User", "Username", true);

            //Create Role table
            var roleEntityBuilder = new RoleEntityBuilder(migrationBuilder, ActiveDatabase);
            roleEntityBuilder.Create();

            //Create UserRole table
            var userRoleEntityBuilder = new UserRoleEntityBuilder(migrationBuilder, ActiveDatabase);
            userRoleEntityBuilder.Create();
            userRoleEntityBuilder.AddIndex("IX_UserRole", new [] {"RoleId", "UserId"}, true);

            //Create Permission table
            var permissionEntityBuilder = new PermissionEntityBuilder(migrationBuilder, ActiveDatabase);
            permissionEntityBuilder.Create();
            permissionEntityBuilder.AddIndex("IX_Permission", new [] {"SiteId", "EntityName", "EntityId", "PermissionName", "RoleId", "UserId"}, true);

            //Create Setting table
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.Create();
            settingEntityBuilder.AddIndex("IX_Setting", new [] {"EntityName", "EntityId", "SettingName"}, true);

            //Create Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.Create();

            //Create Log table
            var logEntityBuilder = new LogEntityBuilder(migrationBuilder, ActiveDatabase);
            logEntityBuilder.Create();

            //Create Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.Create();

            //Create Folder table
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.Create();
            folderEntityBuilder.AddIndex("IX_Folder", new [] {"SiteId", "Path"}, true);

            //Create File table
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.Create();

            //Create AspNetUsers table
            var aspNetUsersEntityBuilder = new AspNetUsersEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUsersEntityBuilder.Create();
            aspNetUsersEntityBuilder.AddIndex("EmailIndex", "NormalizedEmail", true);
            aspNetUsersEntityBuilder.AddIndex("UserNameIndex", "NormalizedUserName", true);

            //Create AspNetUserClaims table
            var aspNetUserClaimsEntityBuilder = new AspNetUserClaimsEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUserClaimsEntityBuilder.Create();
            aspNetUserClaimsEntityBuilder.AddIndex("IX_AspNetUserClaims_UserId", "UserId", true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop AspNetUserClaims table
            var aspNetUserClaimsEntityBuilder = new AspNetUserClaimsEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUserClaimsEntityBuilder.Drop();

            //Drop AspNetUsers table
            var aspNetUsersEntityBuilder = new AspNetUsersEntityBuilder(migrationBuilder, ActiveDatabase);
            aspNetUsersEntityBuilder.Drop();

            //Drop File table
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder, ActiveDatabase);
            fileEntityBuilder.Drop();

            //Drop Folder table
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder, ActiveDatabase);
            folderEntityBuilder.Drop();

            //Drop Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder, ActiveDatabase);
            notificationEntityBuilder.Drop();

            //Drop Log table
            var logEntityBuilder = new LogEntityBuilder(migrationBuilder, ActiveDatabase);
            logEntityBuilder.Drop();

            //Drop Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder, ActiveDatabase);
            profileEntityBuilder.Drop();

            //Drop Setting table
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder, ActiveDatabase);
            settingEntityBuilder.Drop();

            //Drop Permission table
            var permissionEntityBuilder = new PermissionEntityBuilder(migrationBuilder, ActiveDatabase);
            permissionEntityBuilder.Drop();

            //Drop UserRole table
            var userRoleEntityBuilder = new UserRoleEntityBuilder(migrationBuilder, ActiveDatabase);
            userRoleEntityBuilder.Drop();

            //Drop Role table
            var roleEntityBuilder = new RoleEntityBuilder(migrationBuilder, ActiveDatabase);
            roleEntityBuilder.Drop();

            //Drop User table
            var userEntityBuilder = new UserEntityBuilder(migrationBuilder, ActiveDatabase);
            userEntityBuilder.Drop();

            //Drop PageModule table
            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            pageModuleEntityBuilder.Drop();

            //Drop Module table
            var moduleEntityBuilder = new ModuleEntityBuilder(migrationBuilder, ActiveDatabase);
            moduleEntityBuilder.Drop();

            //Drop Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder, ActiveDatabase);
            pageEntityBuilder.Drop();

            //Drop Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder, ActiveDatabase);
            siteEntityBuilder.Drop();
        }
    }
}

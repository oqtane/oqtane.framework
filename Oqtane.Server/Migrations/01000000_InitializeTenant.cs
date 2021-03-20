using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Migrations.EntityBuilders;
using Oqtane.Repository;


namespace Oqtane.Migrations
{
    [DbContext(typeof(TenantDBContext))]
    [Migration("Tenant.01.00.00.00")]
    public class InitializeTenant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Create Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder);
            siteEntityBuilder.Create();

            //Create Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder);
            pageEntityBuilder.Create();
            pageEntityBuilder.AddIndex("IX_Page", new [] {"SiteId", "Path", "UserId"}, true);

            //Create Module table
            var moduleEntityBuilder = new ModuleEntityBuilder(migrationBuilder);
            moduleEntityBuilder.Create();

            //Create PageModule table
            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder);
            pageModuleEntityBuilder.Create();

            //Create User table
            var userEntityBuilder = new UserEntityBuilder(migrationBuilder);
            userEntityBuilder.Create();
            userEntityBuilder.AddIndex("IX_User", "Username", true);

            //Create Role table
            var roleEntityBuilder = new RoleEntityBuilder(migrationBuilder);
            roleEntityBuilder.Create();

            //Create UserRole table
            var userRoleEntityBuilder = new UserRoleEntityBuilder(migrationBuilder);
            userRoleEntityBuilder.Create();
            userRoleEntityBuilder.AddIndex("IX_UserRole", new [] {"RoleId", "UserId"}, true);

            //Create Permission table
            var permissionEntityBuilder = new PermissionEntityBuilder(migrationBuilder);
            permissionEntityBuilder.Create();
            permissionEntityBuilder.AddIndex("IX_Permission", new [] {"SiteId", "EntityName", "EntityId", "PermissionName", "RoleId", "UserId"}, true);

            //Create Setting table
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder);
            settingEntityBuilder.Create();
            settingEntityBuilder.AddIndex("IX_Setting", new [] {"EntityName", "EntityId", "SettingName"}, true);

            //Create Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder);
            profileEntityBuilder.Create();

            //Create Log table
            var logEntityBuilder = new LogEntityBuilder(migrationBuilder);
            logEntityBuilder.Create();

            //Create Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder);
            notificationEntityBuilder.Create();

            //Create Folder table
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder);
            folderEntityBuilder.Create();
            folderEntityBuilder.AddIndex("IX_Folder", new [] {"SiteId", "Path"}, true);

            //Create File table
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder);
            fileEntityBuilder.Create();

            //Create AspNetUsers table
            var aspNetUsersEntityBuilder = new AspNetUsersEntityBuilder(migrationBuilder);
            aspNetUsersEntityBuilder.Create();
            aspNetUsersEntityBuilder.AddIndex("EmailIndex", "NormalizedEmail", true);
            aspNetUsersEntityBuilder.AddIndex("UserNameIndex", "NormalizedUserName", true);

            //Create AspNetUserClaims table
            var aspNetUserClaimsEntityBuilder = new AspNetUserClaimsEntityBuilder(migrationBuilder);
            aspNetUserClaimsEntityBuilder.Create();
            aspNetUserClaimsEntityBuilder.AddIndex("IX_AspNetUserClaims_UserId", "UserId", true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //Drop AspNetUserClaims table
            var aspNetUserClaimsEntityBuilder = new AspNetUserClaimsEntityBuilder(migrationBuilder);
            aspNetUserClaimsEntityBuilder.Drop();

            //Drop AspNetUsers table
            var aspNetUsersEntityBuilder = new AspNetUsersEntityBuilder(migrationBuilder);
            aspNetUsersEntityBuilder.Drop();

            //Drop File table
            var fileEntityBuilder = new FileEntityBuilder(migrationBuilder);
            fileEntityBuilder.Drop();

            //Drop Folder table
            var folderEntityBuilder = new FolderEntityBuilder(migrationBuilder);
            folderEntityBuilder.Drop();

            //Drop Notification table
            var notificationEntityBuilder = new NotificationEntityBuilder(migrationBuilder);
            notificationEntityBuilder.Drop();

            //Drop Log table
            var logEntityBuilder = new LogEntityBuilder(migrationBuilder);
            logEntityBuilder.Drop();

            //Drop Profile table
            var profileEntityBuilder = new ProfileEntityBuilder(migrationBuilder);
            profileEntityBuilder.Drop();

            //Drop Setting table
            var settingEntityBuilder = new SettingEntityBuilder(migrationBuilder);
            settingEntityBuilder.Drop();

            //Drop Permission table
            var permissionEntityBuilder = new PermissionEntityBuilder(migrationBuilder);
            permissionEntityBuilder.Drop();

            //Drop UserRole table
            var userRoleEntityBuilder = new UserRoleEntityBuilder(migrationBuilder);
            userRoleEntityBuilder.Drop();

            //Drop Role table
            var roleEntityBuilder = new RoleEntityBuilder(migrationBuilder);
            roleEntityBuilder.Drop();

            //Drop User table
            var userEntityBuilder = new UserEntityBuilder(migrationBuilder);
            userEntityBuilder.Drop();

            //Drop PageModule table
            var pageModuleEntityBuilder = new PageModuleEntityBuilder(migrationBuilder);
            pageModuleEntityBuilder.Drop();

            //Drop Module table
            var moduleEntityBuilder = new ModuleEntityBuilder(migrationBuilder);
            moduleEntityBuilder.Drop();

            //Drop Page table
            var pageEntityBuilder = new PageEntityBuilder(migrationBuilder);
            pageEntityBuilder.Drop();

            //Drop Site table
            var siteEntityBuilder = new SiteEntityBuilder(migrationBuilder);
            siteEntityBuilder.Drop();
        }
    }
}

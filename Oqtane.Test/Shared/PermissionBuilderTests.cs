using Moq;
using Oqtane.Shared;
using System;
using System.Text.Json;
using Oqtane.Models;
using Xunit;

namespace Oqtane.Test.Shared
{
    public class PermissionBuilderTests
    {
        public PermissionBuilderTests()
        {
        }

        private PermissionBuilder CreatePermissionBuilder()
        {
            return new PermissionBuilder();
        }

        [Fact]
        public void ToString_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var permissionBuilder = this.CreatePermissionBuilder();

            var result = permissionBuilder.ToString();
            Assert.Equal("[]", result);

            permissionBuilder.Permit(Permissions.Edit);
            result = permissionBuilder.ToString();
            Assert.Equal("[]", result);

            permissionBuilder.Permit(Permissions.Edit).GrantTo(Constants.AdminRole).DenyTo(Constants.RegisteredRole);
            result = permissionBuilder.ToString();
            Assert.Equal("[{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators;!Registered Users\"}]", result);

            permissionBuilder.Permit(Permissions.Edit).GrantTo(Constants.AdminRole).DenyTo(Constants.RegisteredRole).GrantTo(Constants.RegisteredRole);
            result = permissionBuilder.ToString();
            Assert.Equal("[{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators;Registered Users\"}]", result);


            permissionBuilder.Permit(Permissions.Edit).GrantTo(Constants.AdminRole).DenyTo(Constants.RegisteredRole).GrantTo(Constants.AdminRole);
            result = permissionBuilder.ToString();
            Assert.Equal("[{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators;!Registered Users\"}]", result);

            permissionBuilder.Permit(Permissions.View).GrantTo(Constants.AllUsersRole);
            result = permissionBuilder.ToString();
            Assert.Equal("[{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators;!Registered Users\"},{\"PermissionName\":\"View\",\"Permissions\":\"All Users\"}]", result);
        }


        [Fact]
        public void Parse_StateUnderTest_ExpectedBehavior()
        {
            var ps = "[{\"PermissionName\":\"View\",\"Permissions\":\"Administrators\"},{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators\"}]";
            var permissionBuilder = PermissionBuilder.Parse(ps);
            var result = permissionBuilder.ToString();
            Assert.Equal(ps, result);

            ps = null;
            permissionBuilder = PermissionBuilder.Parse(ps);
            result = permissionBuilder.ToString();
            Assert.Equal("[]", result);

            ps = "";
            permissionBuilder = PermissionBuilder.Parse(ps);
            result = permissionBuilder.ToString();
            Assert.Equal("[]", result);

            
            
            ps = "garbage sdfjhahriruireh;bfehgrehgiepru";
            result = permissionBuilder.ToString();
            Assert.Throws<JsonException>(() => permissionBuilder = PermissionBuilder.Parse(ps));
        }

        [Fact]
        public void IsAuthorised_StateUnderTest_ExpectedBehavior()
        {
            var permissionBuilder = this.CreatePermissionBuilder();
            var user1 = new User {UserId = 1, Roles = "Administrators;Registered Users"};

            Assert.False(permissionBuilder.IsAuthorized(user1, Permissions.Edit));

            permissionBuilder.Permit(Permissions.Edit).GrantTo(Constants.AdminRole);

            Assert.True(permissionBuilder.IsAuthorized(user1, Permissions.Edit));

            permissionBuilder.Permit(Permissions.Edit).DenyTo(Constants.AdminRole);

            Assert.False(permissionBuilder.IsAuthorized(user1, Permissions.Edit));

            Assert.False(permissionBuilder.IsAuthorized(null, Permissions.Edit));

            permissionBuilder.Permit(Permissions.Edit).GrantTo(Constants.AllUsersRole);

            Assert.True(permissionBuilder.IsAuthorized(null, Permissions.Edit));

            Assert.False(permissionBuilder.IsAuthorized(null, Permissions.View));
        }
    }
}

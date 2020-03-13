using Moq;
using Oqtane.Shared;
using System;
using Xunit;

namespace Oqtane.Test.Shared
{
    public class PermissionBuilderTests
    {
        private MockRepository mockRepository;


        public PermissionBuilderTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
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
            
            
            permissionBuilder.Permit(Permissions.Edit).GrantTo(Constants.AdminRole).DenyTo(Constants.RegisteredRole);

            result = permissionBuilder.ToString();
            Assert.Equal("[{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators;!Registered Users\"}]", result);
            
            permissionBuilder.Permit(Permissions.View).GrantTo(Constants.AllUsersRole);
            result = permissionBuilder.ToString();
            
            Assert.Equal("[{\"PermissionName\":\"Edit\",\"Permissions\":\"Administrators;!Registered Users\"},{\"PermissionName\":\"View\",\"Permissions\":\"All Users\"}]", result);            
            
        }
    }
}

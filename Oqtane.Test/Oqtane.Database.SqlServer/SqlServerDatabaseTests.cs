using Xunit;

namespace Oqtane.Database.SqlServer.Tests
{
    public class SqlServerDatabaseTests
    {
        [Fact()]
        public void VerifyDatabaseTypeName()
        {
            // Arrange & Act
            var database = new SqlServerDatabase();

            // Assert
            Assert.Equal("Oqtane.Database.SqlServer.SqlServerDatabase, Oqtane.Database.SqlServer", database.TypeName);
        }
    }
}

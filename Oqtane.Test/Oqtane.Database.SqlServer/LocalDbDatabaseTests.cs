using Xunit;

namespace Oqtane.Database.SqlServer.Tests
{
    public class LocalDbDatabaseTests
    {
        [Fact()]
        public void VerifyDatabaseTypeName()
        {
            // Arrange & Act
            var database = new LocalDbDatabase();

            // Assert
            Assert.Equal("Oqtane.Database.SqlServer.LocalDbDatabase, Oqtane.Database.SqlServer", database.TypeName);
        }
    }
}

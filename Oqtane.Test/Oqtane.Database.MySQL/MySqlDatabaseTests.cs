using Xunit;

namespace Oqtane.Database.MySQL.Tests
{
    public class PostgreSQLDatabaseTests
    {
        [Fact()]
        public void VerifyDatabaseTypeName()
        {
            // Arrange & Act
            var database = new MySQLDatabase();

            // Assert
            Assert.Equal("Oqtane.Database.MySQL.MySQLDatabase, Oqtane.Database.MySQL", database.TypeName);
        }
    }
}

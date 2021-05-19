using Xunit;

namespace Oqtane.Database.PostgreSQL.Tests
{
    public class PostgreSQLDatabaseTests
    {
        [Fact()]
        public void VerifyDatabaseTypeName()
        {
            // Arrange & Act
            var database = new PostgreSQLDatabase();

            // Assert
            Assert.Equal("Oqtane.Database.PostgreSQL.PostgreSQLDatabase, Oqtane.Database.PostgreSQL", database.TypeName);
        }
    }
}

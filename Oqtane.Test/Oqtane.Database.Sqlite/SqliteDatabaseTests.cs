using Xunit;

namespace Oqtane.Database.Sqlite.Tests
{
    public class SqliteDatabaseTests
    {
        [Fact()]
        public void VerifyDatabaseTypeName()
        {
            // Arrange & Act
            var database = new SqliteDatabase();

            // Assert
            Assert.Equal("Oqtane.Database.Sqlite.SqliteDatabase, Oqtane.Database.Sqlite", database.TypeName);
        }
    }
}

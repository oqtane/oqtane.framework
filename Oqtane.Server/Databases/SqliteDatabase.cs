using Microsoft.EntityFrameworkCore;
using Oqtane.Interfaces;

namespace Oqtane.Repository.Databases
{
    public class SqliteDatabase : IDatabase
    {
        public string FriendlyName => Name;

        public string Name => "Sqlite";

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlite(connectionString);
        }
    }
}

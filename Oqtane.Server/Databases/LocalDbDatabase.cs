using Microsoft.EntityFrameworkCore;
using Oqtane.Interfaces;

namespace Oqtane.Repository.Databases
{
    public class LocalDbDatabase : IDatabase
    {
        public string FriendlyName => "Local Database";
        public string Name => "LocalDB";

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

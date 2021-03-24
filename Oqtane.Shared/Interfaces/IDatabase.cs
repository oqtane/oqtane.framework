using Microsoft.EntityFrameworkCore;

namespace Oqtane.Interfaces
{
    public interface IDatabase
    {
        public string FriendlyName { get; }

        public string Name { get; }

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}

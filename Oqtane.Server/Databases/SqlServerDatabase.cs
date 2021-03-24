using Microsoft.EntityFrameworkCore;
using Oqtane.Interfaces;

namespace Oqtane.Repository.Databases
{
    public class SqlServerDatabase : IDatabase
    {
        public string FriendlyName => "SQL Server";

        public string Name => "SqlServer";

        public DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            return optionsBuilder.UseSqlServer(connectionString);
        }
    }
}

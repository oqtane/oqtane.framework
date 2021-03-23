using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseOqtaneDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder, string databaseType, string connectionString)
        {
            switch (databaseType)
            {
                case "SqlServer":
                    optionsBuilder.UseSqlServer(connectionString);

                    break;
                case "Sqlite":
                    optionsBuilder.UseSqlite(connectionString);
                    break;
            }

            return optionsBuilder;
        }
    }
}

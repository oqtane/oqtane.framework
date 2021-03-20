using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Oqtane.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseOqtaneDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder.UseSqlServer(connectionString);
            //optionsBuilder.UseSqlite("Data Source=Oqtane.db");

            return optionsBuilder;
        }
    }
}

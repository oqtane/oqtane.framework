using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Oqtane.Databases.Interfaces;
// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseOqtaneDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder, IDatabase database, string connectionString)
        {
            database.UseDatabase(optionsBuilder, connectionString);

            return optionsBuilder;
        }
    }
}

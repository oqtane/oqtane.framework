using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Oqtane.Databases.Interfaces;
// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseOqtaneDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder, IDatabase database, string connectionString)
        {
            database.UseDatabase(optionsBuilder, connectionString)
                .ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning));

            return optionsBuilder;
        }
    }
}

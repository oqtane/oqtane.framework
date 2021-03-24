using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Interfaces;
// ReSharper disable ConvertToUsingDeclaration

namespace Oqtane.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseOqtaneDatabase([NotNull] this DbContextOptionsBuilder optionsBuilder, string databaseType, string connectionString)
        {
            var type = Type.GetType(databaseType);
            var database = Activator.CreateInstance(type) as IDatabase;

            database.UseDatabase(optionsBuilder, connectionString);

            return optionsBuilder;
        }
    }
}

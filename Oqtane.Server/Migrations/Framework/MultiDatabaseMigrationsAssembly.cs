using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;
using Oqtane.Repository.Databases.Interfaces;

namespace Oqtane.Migrations.Framework
{
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class MultiDatabaseMigrationsAssembly: MigrationsAssembly
#pragma warning restore EF1001 // Internal EF Core API usage.
    {
        private readonly IDatabase _database;

        public MultiDatabaseMigrationsAssembly(
            ICurrentDbContext currentContext,
            IDbContextOptions options,
            IMigrationsIdGenerator idGenerator,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
#pragma warning disable EF1001 // Internal EF Core API usage.
            : base(currentContext, options, idGenerator, logger)
#pragma warning restore EF1001 // Internal EF Core API usage.
        {
            var multiDatabaseContext = currentContext.Context as IMultiDatabase;
            if (multiDatabaseContext != null) _database = multiDatabaseContext.ActiveDatabase;
        }
        public override Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            var hasCtorWithCacheOptions = migrationClass.GetConstructor(new[] { typeof(IDatabase) }) != null;

            if (hasCtorWithCacheOptions)
            {
                var migration = (Migration)Activator.CreateInstance(migrationClass.AsType(), _database);
                if (migration != null)
                {
                    migration.ActiveProvider = activeProvider;
                    return migration;
                }
            }

#pragma warning disable EF1001 // Internal EF Core API usage.
            return base.CreateMigration(migrationClass, activeProvider);
#pragma warning restore EF1001 // Internal EF Core API usage.
        }
    }
}

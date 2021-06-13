using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Modules
{
    public class MigratableModuleBase
    {
        public bool Migrate(DBContextBase dbContext, Tenant tenant, MigrationType migrationType)
        {
            var result = true;

            using (dbContext)
            {
                try
                {
                    var migrator = dbContext.GetService<IMigrator>();
                    if (migrationType == MigrationType.Down)
                    {
                        migrator.Migrate(Migration.InitialDatabase);
                    }
                    else
                    {
                        migrator.Migrate();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    result = false;
                }

            }
            return result;

        }
    }
}

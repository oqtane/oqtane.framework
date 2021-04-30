using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Models;

namespace Oqtane.Shared
{
    public abstract class OqtaneDatabaseBase : IOqtaneDatabase
    {
        protected OqtaneDatabaseBase(string name, string friendlyName)
        {
            Name = name;
            FriendlyName = friendlyName;
        }

        public  string FriendlyName { get; }

        public string Name { get; }

        public abstract string Provider { get; }

        public abstract OperationBuilder<AddColumnOperation> AddAutoIncrementColumn(ColumnsBuilder table, string name);

        public virtual string ConcatenateSql(params string[] values)
        {
            var returnValue = String.Empty;
            for (var i = 0; i < values.Length; i++)
            {
                if (i > 0)
                {
                    returnValue += " + ";
                }
                returnValue += values[i];
            }

            return returnValue;
        }

        public virtual string RewriteName(string name)
        {
            return name;
        }

        public virtual void UpdateIdentityStoreTableNames(ModelBuilder builder)
        {

        }

        public abstract DbContextOptionsBuilder UseDatabase(DbContextOptionsBuilder optionsBuilder, string connectionString);
    }
}

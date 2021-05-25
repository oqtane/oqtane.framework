using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Oqtane.Migrations.Framework;
using Oqtane.Models;
using Oqtane.Shared;

// ReSharper disable ClassNeverInstantiated.Global

namespace Oqtane.Database.SqlServer
{
    public class OqtaneHistoryRepository : SqlServerHistoryRepository
    {
        private string _appliedDateColumnName = "AppliedDate";
        private string _appliedVersionColumnName = "AppliedVersion";
        private MigrationHistoryTable _migrationHistoryTable;

        public OqtaneHistoryRepository(HistoryRepositoryDependencies dependencies) : base(dependencies)
        {
            _migrationHistoryTable = new MigrationHistoryTable
            {
                TableName = TableName,
                TableSchema = TableSchema,
                MigrationIdColumnName = MigrationIdColumnName,
                ProductVersionColumnName = ProductVersionColumnName,
                AppliedVersionColumnName = _appliedVersionColumnName,
                AppliedDateColumnName = _appliedDateColumnName
            };

        }

        protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
        {
            base.ConfigureTable(history);
            history.Property<string>(_appliedVersionColumnName).HasMaxLength(10);
            history.Property<DateTime>(_appliedDateColumnName);
        }

        public override string GetInsertScript(HistoryRow row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            return MigrationUtils.BuildInsertScript(row, Dependencies, _migrationHistoryTable);
        }
    }
}

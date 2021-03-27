using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class JobEntityBuilder : AuditableBaseEntityBuilder<JobEntityBuilder>
    {
        private const string _entityTableName = "Job";
        private readonly PrimaryKey<JobEntityBuilder> _primaryKey = new("PK_Job", x => x.JobId);

        public JobEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override JobEntityBuilder BuildTable(ColumnsBuilder table)
        {
            JobId = ActiveDatabase.AddAutoIncrementColumn(table,"JobId");
            Name = table.AddStringColumn("Name", 200);
            JobType = table.AddStringColumn("JobType", 200);
            Frequency = table.AddStringColumn("Frequency", 1);
            Interval = table.AddIntegerColumn("Interval");
            StartDate = table.AddDateTimeColumn("StartDate", true);
            EndDate = table.AddDateTimeColumn("EndDate", true);
            IsEnabled = table.AddBooleanColumn("IsEnabled");
            IsStarted = table.AddBooleanColumn("IsStarted");
            IsExecuting = table.AddBooleanColumn("IsExecuting");
            NextExecution = table.AddDateTimeColumn("NextExecution", true);
            RetentionHistory = table.AddIntegerColumn("RetentionHistory");

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> JobId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> JobType { get; set; }

        public OperationBuilder<AddColumnOperation> Frequency { get; set; }

        public OperationBuilder<AddColumnOperation> Interval { get; set; }

        public OperationBuilder<AddColumnOperation> StartDate { get; set; }

        public OperationBuilder<AddColumnOperation> EndDate { get; set; }

        public OperationBuilder<AddColumnOperation> IsEnabled { get; set; }

        public OperationBuilder<AddColumnOperation> IsStarted { get; set; }

        public OperationBuilder<AddColumnOperation> IsExecuting { get; set; }

        public OperationBuilder<AddColumnOperation> NextExecution { get; set; }

        public OperationBuilder<AddColumnOperation> RetentionHistory { get; set; }
    }
}

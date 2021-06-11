using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class JobEntityBuilder : AuditableBaseEntityBuilder<JobEntityBuilder>
    {
        private const string _entityTableName = "Job";
        private readonly PrimaryKey<JobEntityBuilder> _primaryKey = new("PK_Job", x => x.JobId);

        public JobEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override JobEntityBuilder BuildTable(ColumnsBuilder table)
        {
            JobId = AddAutoIncrementColumn(table,"JobId");
            Name = AddStringColumn(table,"Name", 200);
            JobType = AddStringColumn(table,"JobType", 200);
            Frequency = AddStringColumn(table,"Frequency", 1);
            Interval = AddIntegerColumn(table,"Interval");
            StartDate = AddDateTimeColumn(table,"StartDate", true);
            EndDate = AddDateTimeColumn(table,"EndDate", true);
            IsEnabled = AddBooleanColumn(table,"IsEnabled");
            IsStarted = AddBooleanColumn(table,"IsStarted");
            IsExecuting = AddBooleanColumn(table,"IsExecuting");
            NextExecution = AddDateTimeColumn(table,"NextExecution", true);
            RetentionHistory = AddIntegerColumn(table,"RetentionHistory");

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

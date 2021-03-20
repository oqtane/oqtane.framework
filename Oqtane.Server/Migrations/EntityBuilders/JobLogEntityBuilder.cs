using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class JobLogEntityBuilder : BaseEntityBuilder<JobLogEntityBuilder>
    {
        private const string _entityTableName = "JobLog";
        private readonly PrimaryKey<JobLogEntityBuilder> _primaryKey = new("PK_JobLog", x => x.JobLogId);
        private readonly ForeignKey<JobLogEntityBuilder> _jobLogForeignKey = new("FK_JobLog_Job", x => x.JobId, "Job", "JobId", ReferentialAction.Cascade);

        public JobLogEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_jobLogForeignKey);
        }

        protected override JobLogEntityBuilder BuildTable(ColumnsBuilder table)
        {
            JobLogId = table.AddAutoIncrementColumn("JobLogId");
            JobId = table.AddIntegerColumn("JobId");
            StartDate = table.AddDateTimeColumn("StartDate");
            FinishDate = table.AddDateTimeColumn("FinishDate", true);
            Succeeded = table.AddBooleanColumn("Succeeded", true);
            Notes = table.AddMaxStringColumn("Notes", true);

            return this;
        }

        public OperationBuilder<AddColumnOperation> JobLogId { get; private set; }

        public OperationBuilder<AddColumnOperation> JobId { get; private set; }

        public OperationBuilder<AddColumnOperation> StartDate { get; private set; }

        public OperationBuilder<AddColumnOperation> FinishDate { get; private set; }

        public OperationBuilder<AddColumnOperation> Succeeded { get; private set; }

        public OperationBuilder<AddColumnOperation> Notes { get; private set; }
    }
}

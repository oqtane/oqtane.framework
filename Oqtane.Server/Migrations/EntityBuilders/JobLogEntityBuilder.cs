using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class JobLogEntityBuilder : BaseEntityBuilder<JobLogEntityBuilder>
    {
        private const string _entityTableName = "JobLog";
        private readonly PrimaryKey<JobLogEntityBuilder> _primaryKey = new("PK_JobLog", x => x.JobLogId);
        private readonly ForeignKey<JobLogEntityBuilder> _jobLogForeignKey = new("FK_JobLog_Job", x => x.JobId, "Job", "JobId", ReferentialAction.Cascade);

        public JobLogEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_jobLogForeignKey);
        }

        protected override JobLogEntityBuilder BuildTable(ColumnsBuilder table)
        {
            JobLogId = AddAutoIncrementColumn(table,"JobLogId");
            JobId = AddIntegerColumn(table,"JobId");
            StartDate = AddDateTimeColumn(table,"StartDate");
            FinishDate = AddDateTimeColumn(table,"FinishDate", true);
            Succeeded = AddBooleanColumn(table,"Succeeded", true);
            Notes = AddMaxStringColumn(table,"Notes", true);

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

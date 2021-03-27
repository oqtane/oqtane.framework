using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Interfaces;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class LogEntityBuilder : BaseEntityBuilder<LogEntityBuilder>
    {
        private const string _entityTableName = "Log";
        private readonly PrimaryKey<LogEntityBuilder> _primaryKey = new("PK_Log", x => x.LogId);
        private readonly ForeignKey<LogEntityBuilder> _siteForeignKey = new("FK_Log_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public LogEntityBuilder(MigrationBuilder migrationBuilder, IOqtaneDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override LogEntityBuilder BuildTable(ColumnsBuilder table)
        {
            LogId = ActiveDatabase.AddAutoIncrementColumn(table,"LogId");
            SiteId = table.AddIntegerColumn("SiteId", true);
            LogDate = table.AddDateTimeColumn("LogDate");
            PageId = table.AddIntegerColumn("PageId", true);
            ModuleId = table.AddIntegerColumn("ModuleId", true);
            UserId = table.AddIntegerColumn("UserId", true);
            Url = table.AddStringColumn("Url", 2048);
            Server = table.AddStringColumn("Server", 200);
            Category = table.AddStringColumn("Category", 200);
            Feature = table.AddStringColumn("Feature", 200);
            Function = table.AddStringColumn("Function", 20);
            Level = table.AddStringColumn("Level", 20);
            Message = table.AddMaxStringColumn("Message");
            MessageTemplate = table.AddMaxStringColumn("MessageTemplate");
            Exception = table.AddMaxStringColumn("Exception", true);
            Properties = table.AddMaxStringColumn("Properties", true);

            return this;
        }

        public OperationBuilder<AddColumnOperation> LogId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> LogDate { get; set; }

        public OperationBuilder<AddColumnOperation> PageId { get; set; }

        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }

        public OperationBuilder<AddColumnOperation> UserId { get; set; }

        public OperationBuilder<AddColumnOperation> Url { get; set; }

        public OperationBuilder<AddColumnOperation> Server { get; set; }

        public OperationBuilder<AddColumnOperation> Category { get; set; }

        public OperationBuilder<AddColumnOperation> Feature { get; set; }

        public OperationBuilder<AddColumnOperation> Function { get; set; }

        public OperationBuilder<AddColumnOperation> Level { get; set; }

        public OperationBuilder<AddColumnOperation> Message { get; set; }

        public OperationBuilder<AddColumnOperation> MessageTemplate { get; set; }

        public OperationBuilder<AddColumnOperation> Exception { get; set; }

        public OperationBuilder<AddColumnOperation> Properties { get; set; }
    }
}

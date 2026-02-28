using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class FolderConfigEntityBuilder : AuditableBaseEntityBuilder<FolderConfigEntityBuilder>
    {
        private const string _entityTableName = "FolderConfig";
        private readonly PrimaryKey<FolderConfigEntityBuilder> _primaryKey = new("PK_FolderConfig", x => x.FolderConfigId);
        private readonly ForeignKey<FolderConfigEntityBuilder> _siteForeignKey = new("FK_FolderConfig_Site", x => x.SiteId, "Site", "SiteId", ReferentialAction.Cascade);

        public FolderConfigEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_siteForeignKey);
        }

        protected override FolderConfigEntityBuilder BuildTable(ColumnsBuilder table)
        {
            FolderConfigId = AddAutoIncrementColumn(table, "FolderConfigId");
            SiteId = AddIntegerColumn(table,"SiteId");
            Name = AddStringColumn(table,"Name", 50);
            Provider = AddStringColumn(table, "Provider", 50);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> FolderConfigId { get; set; }

        public OperationBuilder<AddColumnOperation> SiteId { get; set; }

        public OperationBuilder<AddColumnOperation> Name { get; set; }

        public OperationBuilder<AddColumnOperation> Provider { get; set; }
    }
}

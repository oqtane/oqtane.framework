using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SiteEntityBuilder : DeletableAuditableBaseEntityBuilder<SiteEntityBuilder>
    {
        private const string _entityTableName = "Site";
        private readonly PrimaryKey<SiteEntityBuilder> _primaryKey = new("PK_Site", x => x.SiteId);

        public SiteEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SiteEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteId = AddAutoIncrementColumn(table,"SiteId");
            TenantId = AddIntegerColumn(table,"TenantId");
            Name = AddStringColumn(table,"Name", 200);
            LogoFileId = AddIntegerColumn(table,"LogoFileId", true);
            FaviconFileId = AddIntegerColumn(table,"FaviconFileId", true);
            DefaultThemeType = AddStringColumn(table,"DefaultThemeType", 200);
            DefaultContainerType = AddStringColumn(table,"DefaultContainerType", 200);
            PwaIsEnabled = AddBooleanColumn(table,"PwaIsEnabled");
            PwaAppIconFileId = AddIntegerColumn(table,"PwaAppIconFileId", true);
            PwaSplashIconFileId = AddIntegerColumn(table,"PwaSplashIconFileId", true);
            AllowRegistration = AddBooleanColumn(table,"AllowRegistration");

            AddAuditableColumns(table);
            AddDeletableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> SiteId { get; private set; }

        public OperationBuilder<AddColumnOperation> TenantId { get; private set; }

        public OperationBuilder<AddColumnOperation> Name { get; private set; }

        public OperationBuilder<AddColumnOperation> LogoFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> FaviconFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> DefaultThemeType { get; private set; }

        public OperationBuilder<AddColumnOperation> DefaultContainerType { get; private set; }

        public OperationBuilder<AddColumnOperation> PwaIsEnabled { get; private set; }

        public OperationBuilder<AddColumnOperation> PwaAppIconFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> PwaSplashIconFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> AllowRegistration { get; private set; }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Migrations.Extensions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class SiteEntityBuilder : DeletableAuditableBaseEntityBuilder<SiteEntityBuilder>
    {
        private const string _entityTableName = "Site";
        private readonly PrimaryKey<SiteEntityBuilder> _primaryKey = new("PK_Site", x => x.SiteId);

        public SiteEntityBuilder(MigrationBuilder migrationBuilder) : base(migrationBuilder)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override SiteEntityBuilder BuildTable(ColumnsBuilder table)
        {
            SiteId = table.AddAutoIncrementColumn("SiteId");
            TenantId = table.AddIntegerColumn("TenantId");
            Name = table.AddStringColumn("Name", 200);
            LogoFileId = table.AddIntegerColumn("LogoFileId", true);
            FaviconFileId = table.AddIntegerColumn("FaviconFileId", true);
            DefaultThemeType = table.AddStringColumn("DefaultThemeType", 200);
            DefaultLayoutType = table.AddStringColumn("DefaultLayoutType", 200);
            DefaultContainerType = table.AddStringColumn("DefaultContainerType", 200);
            PwaIsEnabled = table.AddBooleanColumn("PwaIsEnabled");
            PwaAppIconFileId = table.AddIntegerColumn("PwaAppIconFileId", true);
            PwaSplashIconFileId = table.AddIntegerColumn("PwaSplashIconFileId", true);
            AllowRegistration = table.AddBooleanColumn("AllowRegistration");

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

        public OperationBuilder<AddColumnOperation> DefaultLayoutType { get; private set; }

        public OperationBuilder<AddColumnOperation> DefaultContainerType { get; private set; }

        public OperationBuilder<AddColumnOperation> PwaIsEnabled { get; private set; }

        public OperationBuilder<AddColumnOperation> PwaAppIconFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> PwaSplashIconFileId { get; private set; }

        public OperationBuilder<AddColumnOperation> AllowRegistration { get; private set; }
    }
}

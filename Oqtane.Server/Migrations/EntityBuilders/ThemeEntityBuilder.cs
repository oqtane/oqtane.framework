using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Oqtane.Migrations.EntityBuilders
{
    public class ThemeEntityBuilder : AuditableBaseEntityBuilder<ThemeEntityBuilder>
    {
        private const string _entityTableName = "Theme";
        private readonly PrimaryKey<ThemeEntityBuilder> _primaryKey = new("PK_Theme", x => x.ThemeId);

        public ThemeEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
        }

        protected override ThemeEntityBuilder BuildTable(ColumnsBuilder table)
        {
            ThemeId = AddAutoIncrementColumn(table, "ThemeId");
            ThemeName = AddStringColumn(table, "ThemeName", 200);

            AddAuditableColumns(table);

            return this;
        }

        public OperationBuilder<AddColumnOperation> ThemeId { get; private set; }

        public OperationBuilder<AddColumnOperation> ThemeName { get; private set; }
    }
}

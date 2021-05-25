using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Migrations.Framework
{
    public static class MigrationUtils
    {
        public static string BuildInsertScript(HistoryRow row, HistoryRepositoryDependencies dependencies, MigrationHistoryTable historyTable )
        {
            var sqlGenerationHelper = dependencies.SqlGenerationHelper;
            var stringTypeMapping = dependencies.TypeMappingSource.GetMapping(typeof(string));

            return new StringBuilder().Append("INSERT INTO ")
                .Append(sqlGenerationHelper.DelimitIdentifier(historyTable.TableName, historyTable.TableSchema))
                .Append(" (")
                .Append(sqlGenerationHelper.DelimitIdentifier(historyTable.MigrationIdColumnName))
                .Append(", ")
                .Append(sqlGenerationHelper.DelimitIdentifier(historyTable.ProductVersionColumnName))
                .Append(", ")
                .Append(sqlGenerationHelper.DelimitIdentifier(historyTable.AppliedVersionColumnName))
                .Append(", ")
                .Append(sqlGenerationHelper.DelimitIdentifier(historyTable.AppliedDateColumnName))
                .AppendLine(")")
                .Append("VALUES (")
                .Append(stringTypeMapping.GenerateSqlLiteral(row.MigrationId))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(row.ProductVersion))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(Constants.Version))
                .Append(", ")
                .Append(stringTypeMapping.GenerateSqlLiteral(DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss'.'fffffffK")))
                .Append(")")
                .AppendLine(sqlGenerationHelper.StatementTerminator)
                .ToString();
        }

    }
}

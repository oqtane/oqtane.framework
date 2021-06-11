namespace Oqtane.Models
{
    public class MigrationHistoryTable
    {
        public string TableName { get; set; }
        public string TableSchema { get; set; }
        public string MigrationIdColumnName { get; set; }
        public string ProductVersionColumnName { get; set; }
        public string AppliedVersionColumnName { get; set; }
        public string AppliedDateColumnName { get; set; }
    }
}

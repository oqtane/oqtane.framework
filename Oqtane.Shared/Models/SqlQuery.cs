using System.Collections.Generic;

namespace Oqtane.Models
{
    public class SqlQuery
    {
        public int TenantId { get; set; }
        public string Query { get; set; }
        public List<Dictionary<string, string>> Results { get; set; }
    }
}

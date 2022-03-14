using System;
using System.Collections.Generic;

namespace Oqtane.Models
{
     public interface IAlias
    {
        int AliasId { get; set; }
        string Name { get; set; }
        int TenantId { get; set; }
        int SiteId { get; set; }
        bool IsDefault { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedOn { get; set; }
        string Path { get; }
        string SiteKey { get; } 
        Dictionary<string, string> SiteSettings { get; set; }
    }
}

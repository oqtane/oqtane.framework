using System;

namespace Oqtane.Models
{
    public interface IAuditable
    {
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime ModifiedOn { get; set; }
    }
}

using System;

namespace Oqtane.Models
{
    public interface ISoftDeletable
    {
        string DeletedBy { get; set; }
        DateTime? DeletedOn { get; set; }
        bool IsSoftDeleted { get; set; }
    }
}

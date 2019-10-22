using System;

namespace Oqtane.Models
{
    public interface IDeletable
    {
        string DeletedBy { get; set; }
        DateTime? DeletedOn { get; set; }
        bool IsDeleted { get; set; }
    }
}

using System;

namespace Oqtane.Models
{
    public class Setting : IAuditable
    {
        public int SettingId { get; set; }
        public string EntityName { get; set; }
        public int EntityId { get; set; }
        public string SettingName { get; set; }
        public string SettingValue { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

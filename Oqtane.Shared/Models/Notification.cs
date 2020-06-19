using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    public class Notification : IDeletable
    {
        public int NotificationId { get; set; }
        public int SiteId { get; set; }
        public int? FromUserId { get; set; }
        public string FromDisplayName { get; set; }
        public string FromEmail { get; set; }
        public int? ToUserId { get; set; }
        public string ToDisplayName { get; set; }
        public string ToEmail { get; set; }
        public int? ParentId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredOn { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? SendOn { get; set; }
    }

}

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

        public Notification() {}

        public Notification(int siteId, User from, User to, string subject, string body, int? parentId)
        {
            SiteId = siteId;
            if (from != null)
            {
                FromUserId = from.UserId;
                FromDisplayName = from.DisplayName;
                FromEmail = from.Email;
            }
            if (to != null)
            {
                ToUserId = to.UserId;
                ToDisplayName = to.DisplayName;
                ToEmail = to.Email;
            }
            Subject = subject;
            Body = body;
            ParentId = parentId;
            CreatedOn = DateTime.UtcNow;
            IsDelivered = false;
            DeliveredOn = null;
            SendOn = DateTime.UtcNow;
        }

        public Notification(int siteId, string fromDisplayName, string fromEmail, string toDisplayName, string toEmail, string subject, string body)
        {
            SiteId = siteId;
            FromUserId = null;
            FromDisplayName = fromDisplayName;
            FromEmail = fromEmail;
            ToUserId = null;
            ToDisplayName = toDisplayName;
            ToEmail = toEmail;
            Subject = subject;
            Body = body;
            ParentId = null;
            CreatedOn = DateTime.UtcNow;
            IsDelivered = false;
            DeliveredOn = null;
            SendOn = DateTime.UtcNow;
        }
    }

}

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Notification for a User - usually meant to be sent as an E-Mail.
    /// </summary>
    public class Notification : IDeletable
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int NotificationId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/> on which the Notification was created. 
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Creator <see cref="User"/> ID
        /// </summary>
        public int? FromUserId { get; set; }

        /// <summary>
        /// Nice Name of the Creator
        /// </summary>
        public string FromDisplayName { get; set; }

        /// <summary>
        /// Creator E-Mail
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Recipient <see cref="User"/> ID - nullable, as Recipient could be someone that's not a user. 
        /// </summary>
        public int? ToUserId { get; set; }

        /// <summary>
        /// Recipient Nice-Name.
        /// </summary>
        public string ToDisplayName { get; set; }

        /// <summary>
        /// Recipient Mail
        /// </summary>
        public string ToEmail { get; set; }

        /// <summary>
        /// Reference to an optional Parent <see cref="Notification"/> - in case it's a kind of thread with reply-messages.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Message Subject.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body / Contents of this Notification
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// When the notification was created. 
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// If it has been delivered. See also <see cref="DeliveredOn"/>.
        /// </summary>
        public bool IsDelivered { get; set; }

        /// <summary>
        /// When the Notification was sent/delivered.
        /// </summary>
        public DateTime? DeliveredOn { get; set; }

        #region IDeletable
        
        /// <inheritdoc />
        public string DeletedBy { get; set; }
        /// <inheritdoc />
        public DateTime? DeletedOn { get; set; }
        /// <inheritdoc />
        public bool IsDeleted { get; set; }

        #endregion

        /// <summary>
        /// When the Notification _should_ be sent. See also <see cref="DeliveredOn"/>
        /// </summary>
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

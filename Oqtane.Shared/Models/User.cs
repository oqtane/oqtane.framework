using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a User in Oqtane.
    /// </summary>
    public class User : ModelBase, IDeletable
    {
        /// <summary>
        /// ID of this User.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Username used for login.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Name shown in menus / dialogs etc.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// User E-Mail address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User time zone
        /// </summary>
        public string TimeZoneId { get; set; }

        /// <summary>
        /// Reference to a <see cref="File"/> containing the users photo.
        /// </summary>
        public int? PhotoFileId { get; set; }

        /// <summary>
        /// Timestamp of last login.
        /// </summary>
        public DateTime? LastLoginOn { get; set; }

        /// <summary>
        /// IP address when the user last logged in to this site. 
        /// </summary>
        public string LastIPAddress { get; set; }

        /// <summary>
        /// Indicates if the user requires 2 factor authentication to sign in
        /// </summary>
        public bool TwoFactorRequired { get; set; }

        /// <summary>
        /// Stores the 2 factor verification code
        /// </summary>
        public string TwoFactorCode { get; set; }

        /// <summary>
        /// The expiry date/time for the 2 factor verification code
        /// </summary>
        public DateTime? TwoFactorExpiry { get; set; }

        /// <summary>
        /// A token indicating if a user's security properties have been modified
        /// </summary>
        [NotMapped]
        public string SecurityStamp { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/> this user belongs to.
        /// </summary>
        [NotMapped]
        public int SiteId { get; set; }

        /// <summary>
        /// Semi-colon delimited list of role names for the user
        /// </summary>
        [NotMapped]
        public string Roles { get; set; }

        #region IDeletable Properties

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

        /// <summary>
        /// The users password. Note that this is not plaintext, so you can probably never really work with this. 
        /// </summary>
        [NotMapped]
        public string Password { get; set; }

        /// <summary>
        /// Information if this user is authenticated. Anonymous users are not authenticated.
        /// </summary>
        [NotMapped]
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// The path name of the user's personal folder
        /// </summary>
        [NotMapped]
        public string FolderPath
        {
            get => "Users/" + UserId.ToString() + "/";
        }

        /// <summary>
        /// Information if this user's email address is confirmed (set during user creation)
        /// </summary>
        [NotMapped]
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Indicates if new user should be notified by email (set during user creation)
        /// </summary>
        [NotMapped]
        public bool SuppressNotification { get; set; }

        /// <summary>
        /// Public User Settings
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }
    }
}

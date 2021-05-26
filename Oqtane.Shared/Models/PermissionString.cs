namespace Oqtane.Models
{
    /// <summary>
    /// Use this to define a <see cref="PermissionName"/> which addresses a set of multiple permissions.
    /// </summary>
    public class PermissionString
    {
        /// <summary>
        /// A term describing a set of permissions
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// The permissions addressed with this name
        /// </summary>
        public string Permissions { get; set; }
    }
}

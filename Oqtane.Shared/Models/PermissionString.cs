namespace Oqtane.Models
{
    /// <summary>
    /// Use this to define a <see cref="PermissionName"/> which addresses a set of multiple permissions.
    /// </summary>
    public class PermissionString
    {
        /// <summary>
        /// A term describing the entity
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// A term describing a set of permissions
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// The permissions
        /// </summary>
        public string Permissions { get; set; }
    }
}

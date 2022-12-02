namespace Oqtane.Models
{
    /// <summary>
    /// API management 
    /// </summary>
    public class Api
    {
        /// <summary>
        /// Reference to a <see cref="Site"/> 
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// The Entity Name
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// The permissions for the entity
        /// </summary>
        public string Permissions { get; set; }
    }
}

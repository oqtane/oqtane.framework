namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Storage Location
    /// </summary>
    public class StorageLocation : ModelBase
    {
        /// <summary>
        /// ID to identify the storage location
        /// </summary>
        public int StorageLocationId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Storage Location Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Storage Provider Type
        /// </summary>
        public string Provider { get; set; }
    }
}

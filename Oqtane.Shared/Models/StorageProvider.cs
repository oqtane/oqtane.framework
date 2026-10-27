
namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Storage Provider
    /// </summary>
    public class StorageProvider
    {
        /// <summary>
        /// Fully Qualified Type Name
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Settings Control Type
        /// </summary>
        public string ControlType { get; set; }

        /// <summary>
        /// Display Name
        /// </summary>
        public string DisplayName { get; set; }
    }
}

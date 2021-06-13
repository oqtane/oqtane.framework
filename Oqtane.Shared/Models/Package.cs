namespace Oqtane.Models
{
    /// <summary>
    /// A software Package which is like an Oqtane Plugin / Extension.
    /// This is used for creating lists from NuGet to offer for installation. 
    /// </summary>
    public class Package
    {
        /// <summary>
        /// ID of the Package. Usually constructed of the Namespace.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Name of the package - may contains part or the entire Namespace.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Nice description of the Package. 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Owner / Creator of the package - usually retrieved from NuGet.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Version as defined in the NuGet package. 
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Download count on NuGet to show how popular the package is. 
        /// </summary>
        public long Downloads { get; set; }
    }
}

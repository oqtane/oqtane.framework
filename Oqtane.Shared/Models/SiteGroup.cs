namespace Oqtane.Models
{
    public class SiteGroup : ModelBase
    {
        /// <summary>
        /// ID to identify the group
        /// </summary>
        public int SiteGroupId { get; set; }

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Group type ie. Synchronization, Localization
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// SiteId of the primary site in the group 
        /// </summary>
        public int PrimarySiteId { get; set; }

        /// <summary>
        /// Specifies if the group should be synchronized
        /// </summary>
        public bool Synchronize{ get; set; }
    }
}

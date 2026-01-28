namespace Oqtane.Models
{
    public class SiteGroupDefinition : ModelBase
    {
        /// <summary>
        /// ID to identify the group
        /// </summary>
        public int SiteGroupDefinitionId { get; set; }

        /// <summary>
        /// Name of the group
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// SiteId of the primary site in the group 
        /// </summary>
        public int PrimarySiteId { get; set; }

        /// <summary>
        /// Indicates if the group supports synchronization (null = no, false = compare, true = update)
        /// </summary>
        public bool? Synchronization { get; set; }

        /// <summary>
        /// Indicates if the site administrator should be notified of any synchronization activity
        /// </summary>
        public bool Notify { get; set; }

        /// <summary>
        /// Specifies if the group needs to be synchronized
        /// </summary>
        public bool Synchronize { get; set; }

        /// <summary>
        /// Indicates if the group supports localization
        /// </summary>
        public bool Localization { get; set; }
    }
}

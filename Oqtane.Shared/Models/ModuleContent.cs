namespace Oqtane.Models
{
    /// <summary>
    /// Generic Model for Module-Contents to enable Import/Export of the Module-Data
    /// </summary>
    public class ModuleContent
    {
        /// <summary>
        /// Reference to a <see cref="ModuleDefinition"/> for which this content is relevant. 
        /// </summary>
        public string ModuleDefinitionName { get; set; }

        /// <summary>
        /// Version of the <see cref="ModuleDefinition"/> which is used here. _It's not the version of the Content_
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Serialized Content of the module for import/export.
        /// </summary>
        public string Content { get; set; }
    }
}

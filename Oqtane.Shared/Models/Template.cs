namespace Oqtane.Models
{
    /// <summary>
    /// model for defining metadata for a Module or Theme template
    /// </summary>
    public class Template
    {
        /// <summary>
        /// name of template (folder name)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// title of template
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// type of template - Internal / External
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// minimum framework version dependency
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// location where template will be created (dynamically set)
        /// </summary>
        public string Location { get; set; }
    }
}

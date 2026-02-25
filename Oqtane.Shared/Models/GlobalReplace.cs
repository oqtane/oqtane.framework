namespace Oqtane.Models
{
    /// <summary>
    /// Describes a global replace operation
    /// </summary>
    public class GlobalReplace
    {
        /// <summary>
        /// text to replace
        /// </summary>
        public string Find { get; set; }

        /// <summary>
        /// replacement text
        /// </summary>
        public string Replace { get; set; }

        /// <summary>
        /// case sensitive
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// include site properties
        /// </summary>
        public bool Site { get; set; }

        /// <summary>
        /// include page properties
        /// </summary>
        public bool Pages { get; set; }

        /// <summary>
        /// include module properties
        /// </summary>
        public bool Modules { get; set; }

        /// <summary>
        /// include content
        /// </summary>
        public bool Content { get; set; }
    }
}

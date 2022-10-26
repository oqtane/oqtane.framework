using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Information about a <see cref="Module"/> instance on a <see cref="Page"/>
    /// </summary>
    public class PageModule : ModelBase, IDeletable
    {
        /// <summary>
        /// Internal ID to identify this instance.
        /// </summary>
        public int PageModuleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Page"/>.
        /// </summary>
        public int PageId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Module"/>.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Module title. Will be shown in the Container if the container shows titles. 
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The Pane which this module instance appears.
        /// </summary>
        public string Pane { get; set; }

        /// <summary>
        /// The sorting order / position in the Pane where this module appears. 
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Reference to a Razor Container which wraps this module instance.
        /// </summary>
        public string ContainerType { get; set; }

        #region IDeletable Properties

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

        /// <summary>
        /// The <see cref="Module"/> itself.
        /// TODO: todoc - unclear if this is always populated
        /// </summary>
        public Module Module { get; set; }
    }
}

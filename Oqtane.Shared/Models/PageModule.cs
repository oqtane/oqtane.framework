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

        /// <summary>
        /// Start of when this assignment is valid. See also <see cref="ExpiryDate"/>
        /// </summary>
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// End of when this assignment is valid. See also <see cref="EffectiveDate"/>
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Header content to include above the module instance in the UI
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Footer content to include below the module instance in the UI
        /// </summary>
        public string Footer { get; set; }

        #region IDeletable Properties

        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public bool IsDeleted { get; set; }

        #endregion

        /// <summary>
        /// The <see cref="Module"/> itself.
        /// </summary>
        public Module Module { get; set; }

        /// <summary>
        /// The <see cref="Page"/> itself.
        /// </summary>
        public Page Page { get; set; }
    }
}

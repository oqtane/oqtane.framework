using System;

namespace Oqtane.Models
{
    /// <summary>
    /// Language Information for <see cref="Site"/>s
    /// TODO: todoc - unclear how this is different from <see cref="Culture"/>
    /// </summary>
    public class Language : IAuditable
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Reference to a <see cref="Site"/>
        /// TODO: todoc - unclear why it's nullable
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Language Name - corresponds to <see cref="Culture.DisplayName"/>, _not_ <see cref="Culture.Name"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Language / Culture code, like 'en-US' - corresponds to <see cref="Culture.Name"/>
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Is this the default language on a <see cref="Site"/>
        /// </summary>
        public bool IsDefault { get; set; }

        #region IAuditable Properties

        /// <inheritdoc/>
        public string CreatedBy { get; set; }

        /// <inheritdoc/>
        public DateTime CreatedOn { get; set; }

        /// <inheritdoc/>
        public string ModifiedBy { get; set; }

        /// <inheritdoc/>
        public DateTime ModifiedOn { get; set; }
        
        #endregion
    }
}

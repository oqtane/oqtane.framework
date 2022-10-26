using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Oqtane.Models
{
    /// <summary>
    /// Language Information for <see cref="Site"/>s
    /// TODO: todoc - unclear how this is different from <see cref="Culture"/>
    /// </summary>
    public class Language : ModelBase
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

        [NotMapped]
        /// <summary>
        /// Version of the satellite assembly
        /// </summary>
        public string Version { get; set; }
    }
}

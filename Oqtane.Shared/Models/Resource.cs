using System;
using Oqtane.Shared;

namespace Oqtane.Models
{
    /// <summary>
    /// Resource Objects describe a JavaScript or CSS file which is needed by the Module to work. 
    /// </summary>
    public class Resource
    {
        private string _url;

        /// <summary>
        /// A <see cref="ResourceType"/> so the Interop can properly create `script` or `link` tags
        /// </summary>
        public ResourceType ResourceType { get; set; }

        /// <summary>
        /// Path to the resources. 
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                _url = (value.Contains("://")) ? value : (!value.StartsWith("/") ? "/" : "") + value;
            }
        }

        /// <summary>
        /// Integrity checks to increase the security of resources accessed. Especially common in CDN resources. 
        /// </summary>
        public string Integrity { get; set; }

        /// <summary>
        /// Cross-Origin rules for this Resources. Usually `anonymous`
        /// </summary>
        public string CrossOrigin { get; set; }

        /// <summary>
        /// Bundle ID in case this Resource belongs to a set of Resources, which may have already been loaded using LoadJS
        /// </summary>
        public string Bundle { get; set; }

        /// <summary>
        /// For Stylesheets this defines the relative position for cascading purposes
        /// </summary>
        public ResourceLevel Level { get; set; }

        /// <summary>
        /// For Scripts this defines if the resource should be included in the Head or Body
        /// </summary>
        public ResourceLocation Location { get; set; }

        /// <summary>
        /// For Scripts this allows type="module" registrations - not applicable to Stylesheets
        /// </summary>
        public bool ES6Module { get; set; }


        [Obsolete("ResourceDeclaration is deprecated", false)]
        public ResourceDeclaration Declaration { get; set; }
    }
}

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
        /// Path to the resource (note that querytring parameters can be included for cache busting ie. ?v=#)
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                _url = (value.Contains("://")) ? value : (!value.StartsWith("/") && !value.StartsWith("~") ? "/" : "") + value;
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
        /// For Scripts a Bundle can be used to identify dependencies and ordering in the script loading process
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

        /// <summary>
        /// Allows specification of inline script - not applicable to Stylesheets
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// For Scripts this defines the render mode (default is all render modes) - not applicable to Stylesheets
        /// </summary>
        public string RenderMode { get; set; }

        /// <summary>
        /// Indicates that a script should be reloaded on every page transition - not applicable to Stylesheets
        /// </summary>
        public bool Reload { get; set; }

        /// <summary>
        /// The namespace of the component that declared the resource - only used in SiteRouter
        /// </summary>
        public string Namespace { get; set; }

        public Resource Clone(ResourceLevel level, string name)
        {
            var resource = new Resource();
            resource.ResourceType = ResourceType;
            resource.Url = Url;
            resource.Integrity = Integrity;
            resource.CrossOrigin = CrossOrigin;
            resource.Bundle = Bundle;
            resource.Location = Location;
            resource.ES6Module = ES6Module;
            resource.Content = Content;
            resource.RenderMode = RenderMode;
            resource.Reload = Reload;
            resource.Level = level;
            resource.Namespace = name;
            return resource;
        }

        [Obsolete("ResourceDeclaration is deprecated", false)]
        public ResourceDeclaration Declaration { get; set; }
    }
}

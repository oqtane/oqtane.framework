using System;
using System.Collections.Generic;
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
        /// A <see cref="ResourceType"/> to define the type of resource ie. Script or Stylesheet
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
        /// For Scripts this allows type to be specified - not applicable to Stylesheets
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Integrity checks to increase the security of resources accessed. Especially common in CDN resources. 
        /// </summary>
        public string Integrity { get; set; }

        /// <summary>
        /// Cross-Origin rules for this Resources. Usually `anonymous`
        /// </summary>
        public string CrossOrigin { get; set; }

        /// <summary>
        /// For Scripts a Bundle can be used to identify dependencies and ordering in the script loading process (for Interactive rendering only)
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
        /// For Scripts this allows for the specification of inline script - not applicable to Stylesheets
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// For Scripts this defines the render mode (default is all render modes) - not applicable to Stylesheets
        /// </summary>
        public string RenderMode { get; set; }

        /// <summary>
        /// Specifies how a script should be loaded in Static rendering - not applicable to Stylesheets
        /// </summary>
        public ResourceLoadBehavior LoadBehavior { get; set; }

        /// <summary>
        /// Cusotm data-* attributes for scripts - not applicable to Stylesheets
        /// </summary>
        public Dictionary<string, string> DataAttributes { get; set; }

        /// <summary>
        /// The namespace of the component that declared the resource - only used in SiteRouter
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Unique identifier of the version of the theme or module that declared the resource - for cache busting - only used in SiteRouter
        /// </summary>
        public string Fingerprint
        {
            set
            {
                // add the fingerprint to the url if it does not contain a querystring already
                if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(Url) && !Url.Contains("?"))
                {
                    Url += "?v=" + value;
                }
            }
        }

        public Resource Clone(ResourceLevel level, string name, string fingerprint)
        {
            var resource = new Resource();
            resource.ResourceType = ResourceType;
            resource.Url = Url;
            resource.Type = Type;
            resource.Integrity = Integrity;
            resource.CrossOrigin = CrossOrigin;
            resource.Bundle = Bundle;
            resource.Location = Location;
            resource.Content = Content;
            resource.RenderMode = RenderMode;
            resource.LoadBehavior = LoadBehavior;
            resource.DataAttributes = new Dictionary<string, string>();
            if (DataAttributes != null && DataAttributes.Count > 0)
            {
                foreach (var kvp in DataAttributes)
                {
                    resource.DataAttributes.Add(kvp.Key, kvp.Value);
                }
            }
            resource.Level = level;
            resource.Namespace = name;
            resource.Fingerprint = fingerprint;
            return resource;
        }

        [Obsolete("ResourceDeclaration is deprecated", false)]
        public ResourceDeclaration Declaration { get; set; }

        [Obsolete("ES6Module is deprecated. Use Type property instead for scripts.", false)]
        public bool ES6Module
        {
            get => (Type == "module");
            set
            {
                if (value)
                {
                    Type = "module";
                };
            }
        }

        [Obsolete("Reload is deprecated. Use LoadBehavior property instead for scripts.", false)]
        public bool Reload
        {
            get => (LoadBehavior == ResourceLoadBehavior.BlazorPageScript);
            set
            {
                if (value)
                {
                    LoadBehavior = ResourceLoadBehavior.BlazorPageScript;
                };
            }
        }
    }
}

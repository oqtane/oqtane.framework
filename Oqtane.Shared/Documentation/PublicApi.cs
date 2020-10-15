using System;

namespace Oqtane.Documentation
{
    /// <summary>
    /// This attribute marks classes, properties etc. as public APIs. 
    /// Any API / code with this attribute will be published in the docs.
    /// You can apply it to anything, but usually you will only need it on classes. 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [PublicApi]
    public class PublicApi: Attribute
    {
        /// <summary>
        /// The `[PublicApi]` attribute can usually be used without additional comment. 
        /// </summary>
        // Important note - this constructor looks unnecessary, because comment could be optional in the other constructor
        // but we need it because of a minor issue in docfx
        public PublicApi() { }

        /// <summary>
        /// Constructor with optional comment `[PublicApi(some-comment)]`
        /// </summary>
        /// <param name="comment">Reason why it's public, optional</param>
        public PublicApi(string comment) { }
    }
}

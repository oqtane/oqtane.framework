using System;

namespace Oqtane.Documentation
{
    /// <summary>
    /// This attribute marks classes, methods, etc. as private APIs
    /// So they should _not_ be publicly documented.
    /// By default, all APIs are private, so you only need this attribute on children of classes marked with `[PublicApi]`.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [PublicApi]
    public class PrivateApi: Attribute
    {
        /// <summary>
        /// The `[PrivateApi]` attribute can be used without additional comment. 
        /// </summary>
        // Important note - this constructor looks unnecessary, because comment could be optional in the other constructor
        // but we need it because of a minor issue in docfx
        public PrivateApi() { }

        /// <summary>
        /// Constructor with optional comment `[PrivateApi(some-comment)]`. 
        /// </summary>
        /// <param name="comment">Reason why it's private, optional</param>
        public PrivateApi(string comment) { }
    }
}

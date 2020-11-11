using System;

namespace Oqtane.Documentation
{
    /// <summary>
    /// This attribute serves as metadata for other things to mark them as internal APIs.
    /// Use this on stuff you want to document publicly, but mark as internal so people are warned
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    [PublicApi]
    // ReSharper disable once InconsistentNaming
    public class InternalApi_DoNotUse_MayChangeWithoutNotice: Attribute
    {
        /// <summary>
        /// The `[InternalApi_DoNotUse_MayChangeWithoutNotice]` attribute can be used without additional comment. 
        /// </summary>
        // Important note - this constructor looks unnecessary, because comment is optional in the other constructor
        // but we need it because of a minor issue in docfx
        public InternalApi_DoNotUse_MayChangeWithoutNotice() { }

        /// <summary>
        /// Constructor with optional comment `[InternalApi_DoNotUse_MayChangeWithoutNotice(some-comment)]`. 
        /// </summary>
        /// <param name="comment">Reason why it's internal, optional</param>
        public InternalApi_DoNotUse_MayChangeWithoutNotice(string comment) { }

    }
}

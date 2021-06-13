namespace Oqtane.Models
{
    /// <summary>
    /// Culture information describing a Culture
    /// </summary>
    public class Culture
    {
        /// <summary>
        /// Short code like `en` or `en-US`
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Nice name for the user, like `English (United States)`
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Information if this is the default culture. 
        /// </summary>
        /// <remarks>
        /// Not sure if this is actually valid, as ATM there is no setting to configure a _default_ culture
        /// </remarks>
        public bool IsDefault { get; set; }
    }
}

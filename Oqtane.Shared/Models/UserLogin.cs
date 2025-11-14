namespace Oqtane.Models
{
    /// <summary>
    /// Passkey properties 
    /// </summary>
    public class UserLogin
    {
        /// <summary>
        ///  the login provider for this login
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// The key for this login
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The friendly name for the login provider
        /// </summary>
        public string Name { get; set; }       
    }
}

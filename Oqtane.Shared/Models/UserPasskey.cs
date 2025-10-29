namespace Oqtane.Models
{
    /// <summary>
    /// Passkey properties 
    /// </summary>
    public class UserPasskey
    {
        /// <summary>
        ///  the credential ID for this passkey 
        /// </summary>
        public byte[] CredentialId { get; set; }

        /// <summary>
        /// The friendly name of the passkey
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The User which this passkey belongs to
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// A serialized JSON object from the navigator.credentials.create() JavaScript API - only populated during Add
        /// </summary>
        public string CredentialJson { get; set; }       
    }
}

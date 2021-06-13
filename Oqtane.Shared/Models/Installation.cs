namespace Oqtane.Models
{
    /// <summary>
    /// Internal message used internally during installation.
    ///
    /// Each part of the installation will return the status / message when installing.
    /// </summary>
    public class Installation 
    {
        /// <summary>
        /// Status if everything worked. 
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message or error in case something failed.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// current alias value from server
        /// </summary>
        public Alias Alias { get; set; }
    }
}

namespace Oqtane.Models
{
    /// <summary>
    /// Helper class for input fields in the setup of the SQL connection
    /// </summary>
    public class ConnectionStringField
    {
        /// <summary>
        /// Simple to understand field name / label
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Instructions / help for the user
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Property / Setting name which will be filled in by the user. Typically something like `Server`, `Port` etc.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initial value used in the UI and probably later also the user input that was given.
        /// </summary>
        public string Value { get; set; }
    }
}

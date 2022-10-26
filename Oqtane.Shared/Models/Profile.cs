using System;

namespace Oqtane.Models
{
    /// <summary>
    /// A single Profile Property information of a <see cref="User"/>.  
    /// So a user will have many of these to fully describe his Profile. 
    /// </summary>
    public class Profile : ModelBase
    {
        /// <summary>
        /// Internal ID
        /// </summary>
        public int ProfileId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>.
        /// It's nullable, probably because certain users like Super-Users don't specifically belong to any site.
        /// </summary>
        public int? SiteId { get; set; }

        /// <summary>
        /// Name of the Profile-Property. _NOT_ the User-Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Title (label) of the Profile Property.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of the Property for the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category of this Profile-Property for grouping etc.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Order of the Property in the list of Profile-Properties.
        /// </summary>
        public int ViewOrder { get; set; }

        /// <summary>
        /// Limits the input length of the property. 
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// Initial/default value of this Property. 
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Some Profile Properties are required - marked with this field.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Some Profile Properties are private, meaning other users won't see them. 
        /// </summary>
        public bool IsPrivate { get; set; }

        /// <summary>
        /// This gives possible values for dropdown input fields. 
        /// </summary>
        public string Options { get; set; }
    }
}

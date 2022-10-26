using System;

namespace Oqtane.Models
{
    /// <summary>
    /// A setting for any kind of object like <see cref="Tenant"/>, <see cref="Site"/>, <see cref="Page"/>, <see cref="Module"/> etc.
    /// </summary>
    public class Setting : ModelBase
    {
        /// <summary>
        /// ID in the Database - mainly used to later update an existing setting. 
        /// </summary>
        public int SettingId { get; set; }

        /// <summary>
        /// What kind of entity the setting is for, like `Page`, `Site` etc.
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Id of the Entity we're describing - so it could be `Site` number 2
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Name of the setting.
        /// </summary>
        public string SettingName { get; set; }

        /// <summary>
        /// The value of this Setting. It's always a string, so make sure to convert/cast as needed.
        /// </summary>
        public string SettingValue { get; set; }

        /// <summary>
        /// Indicates if this setting is private - indicating it should be maintained on the server and not sent to the client
        /// </summary>
        public bool IsPrivate { get; set; }
    }
}

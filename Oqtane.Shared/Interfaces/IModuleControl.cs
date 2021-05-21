using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;

namespace Oqtane.Modules
{
    public interface IModuleControl
    {
        /// <summary>
        /// Defines the security access level for this control - defaults to View
        /// </summary>
        SecurityAccessLevel SecurityAccessLevel { get; }

        /// <summary>
        /// Title to display for this control - defaults to module title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Allows for routing by configuration rather than by convention ( comma delimited ) - defaults to using component file name
        /// </summary>
        string Actions { get; }

        /// <summary>
        /// Container for embedding module control - defaults to true. false will suppress the default modal UI popup behavior and render the component in the page.
        /// </summary>
        bool UseAdminContainer { get; }

        /// <summary>
        /// Identifies all resources in a module
        /// </summary>
        List<Resource> Resources { get; }
    }
}

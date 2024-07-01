using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// The component types which need to register it's own resources.
        /// </summary>
        List<string> ResourcesRegistrationTypes { get; }

        /// <summary>
        /// Identifies all resources in a module including resources from resources registration types.
        /// </summary>
        /// <returns></returns>
        Task<List<Resource>> GetResources(IServiceProvider serviceProvider, Page page);

        /// <summary>
        /// Specifies the required render mode for the module control ie. Static,Interactive
        /// </summary>
        string RenderMode { get; }

        /// <summary>
        /// Specifies the prerender mode for the moudle control ie: true or false
        /// </summary>
        bool? Prerender { get;  }
    }
}

using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Oqtane.Models
{
    /// <summary>
    /// Describes a Module _Instance_ which will be shown on a page. This is different from a <see cref="ModuleDefinition"/> which describes a Module.
    /// </summary>
    public class Module : ModelBase
    {
        /// <summary>
        /// The ID of this instance
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Site"/>
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// Reference to the <see cref="ModuleDefinition"/>
        /// </summary>
        public string ModuleDefinitionName { get; set; }

        /// <summary>
        /// Determines if this module should be shown on all pages of the current <see cref="Site"/>
        /// </summary>
        public bool AllPages { get; set; }


        /// <summary>
        /// Reference to the <see cref="ModuleDefinition"/> used for this module.
        /// </summary>
        [NotMapped]
        public ModuleDefinition ModuleDefinition { get; set; }

        #region IDeletable Properties

        [NotMapped]
        public string DeletedBy { get; set; }
        [NotMapped]
        public DateTime? DeletedOn { get; set; }
        [NotMapped]
        public bool IsDeleted { get; set; }

        #endregion

        /// <summary>
        /// list of permissions for this module
        /// </summary>
        [NotMapped]
        public List<Permission> PermissionList { get; set; }

        /// <summary>
        /// List of settings for this module
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Settings { get; set; }

        #region PageModule properties

        /// <summary>
        /// The id of the PageModule instance
        /// </summary>
        [NotMapped]
        public int PageModuleId { get; set; }

        /// <summary>
        /// Reference to the <see cref="Page"/> this module is on.
        /// </summary>
        [NotMapped]
        public int PageId { get; set; }

        /// <summary>
        /// Title of the pagemodule instance
        /// </summary>
        [NotMapped]
        public string Title { get; set; }

        /// <summary>
        /// The pane where this pagemodule instance will be injected on the page
        /// </summary>
        [NotMapped]
        public string Pane { get; set; }

        /// <summary>
        /// The order of the pagemodule instance within the Pane
        /// </summary>
        [NotMapped]
        public int Order { get; set; }

        /// <summary>
        /// The container for the pagemodule instance
        /// </summary>
        [NotMapped]
        public string ContainerType { get; set; }

        /// <summary>
        /// Start of when this module is visible. See also <see cref="ExpiryDate"/>
        /// </summary>
        [NotMapped]
        public DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// End of when this module is visible. See also <see cref="EffectiveDate"/>
        /// </summary>
        [NotMapped]
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Header content to include at the top of a module instance in the UI
        /// </summary>
        [NotMapped]
        public string Header { get; set; }

        /// <summary>
        /// Footer content to include below a module instance in the UI
        /// </summary>
        [NotMapped]
        public string Footer { get; set; }

        #endregion

        #region SiteRouter properties

        /// <summary>
        /// Stores the type name for the module component being rendered
        /// </summary>
        [NotMapped]
        public string ModuleType { get; set; }

        /// <summary>
        /// The position of the module instance in a pane
        /// </summary>
        [NotMapped]
        public int PaneModuleIndex { get; set; }

        /// <summary>
        /// The number of modules in a pane
        /// </summary>
        [NotMapped]
        public int PaneModuleCount { get; set; }

        /// <summary>
        /// A unique id to help determine if a component should be rendered
        /// </summary>
        [NotMapped]
        public Guid RenderId { get; set; }

        #endregion

        #region IModuleControl properties

        /// <summary>
        /// The minimum access level to view the component being rendered
        /// </summary>
        [NotMapped]
        public SecurityAccessLevel SecurityAccessLevel { get; set; }

        /// <summary>
        /// An optional title for the component
        /// </summary>
        [NotMapped]
        public string ControlTitle { get; set; }

        /// <summary>
        /// Optional mapping of Url actions to a component
        /// </summary>
        [NotMapped]
        public string Actions { get; set; }

        /// <summary>
        /// Optionally indicate if a compoent should not be rendered with the default modal admin container
        /// </summary>
        [NotMapped]
        public bool UseAdminContainer { get; set; }

        /// <summary>
        /// Optionally specify the render mode for the component (overrides the Site setting)
        /// </summary>
        [NotMapped]
        public string RenderMode { get; set; }

        /// <summary>
        /// Optionally specify id the component should be prerendered (overrides the Site setting)
        /// </summary>
        [NotMapped]
        public bool? Prerender { get; set; }

        #endregion

        #region Deprecated Properties

        [Obsolete("The Permissions property is deprecated. Use PermissionList instead", false)]
        [NotMapped]
        [JsonIgnore] // exclude from API payload
        public string Permissions
        {
            get
            {
                return JsonSerializer.Serialize(PermissionList);
            }
            set
            {
                PermissionList = JsonSerializer.Deserialize<List<Permission>>(value);
            }
        }

        #endregion

        public Module Clone()
        {
            return new Module
            {
                ModuleId = ModuleId,
                SiteId = SiteId,
                ModuleDefinitionName = ModuleDefinitionName,
                AllPages = AllPages,
                PageModuleId = PageModuleId,
                PageId = PageId,
                Title = Title,
                Pane = Pane,
                Order = Order,
                ContainerType = ContainerType,
                EffectiveDate = EffectiveDate,
                ExpiryDate = ExpiryDate,
                Header = Header,
                Footer = Footer,
                CreatedBy = CreatedBy,
                CreatedOn = CreatedOn,
                ModifiedBy = ModifiedBy,
                ModifiedOn = ModifiedOn,
                DeletedBy = DeletedBy,
                DeletedOn = DeletedOn,
                IsDeleted = IsDeleted,
                ModuleDefinition = ModuleDefinition, 
                PermissionList = PermissionList.ConvertAll(permission => permission.Clone()),
                Settings = Settings.ToDictionary(setting => setting.Key, setting => setting.Value)
            };
        }
    }
 }

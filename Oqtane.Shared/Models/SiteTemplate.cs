using System;
using System.Collections.Generic;
using System.Text.Json;
using Oqtane.Shared;

namespace Oqtane.Models
{
    public class SiteTemplate
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
    }

    public class PageTemplate
    {
        public PageTemplate()
        {
            Url = "";
            Name = "";
            Parent = "";
            Title = "";
            Path = "";
            Order = 1;
            ThemeType = "";
            DefaultContainerType = "";
            HeadContent = "";
            BodyContent = "";
            Icon = "";
            IsNavigation = true;
            IsClickable = true;
            IsPersonalizable = false;
            IsDeleted = false;
            PermissionList = new List<Permission>()
            {
                new Permission(PermissionNames.View, RoleNames.Admin, true),
                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
            };
            PageTemplateModules = new List<PageTemplateModule>();

            // properties used by IModule
            AliasName = "";
            Version = "*";
            Update = false;
        }

        public string Path { get; set; }
        // note that Parent actually means Parent Path
        public string Parent { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public string Url { get; set; }
        public string ThemeType { get; set; }
        public string DefaultContainerType { get; set; }
        public string HeadContent { get; set; }
        public string BodyContent { get; set; }
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public bool IsClickable { get; set; }
        public bool IsPersonalizable { get; set; }
        public bool IsDeleted { get; set; }
        public List<Permission> PermissionList { get; set; }
        public List<PageTemplateModule> PageTemplateModules { get; set; }

        // properties used by IModule
        public string AliasName { get; set; }
        public string Version { get; set; }
        public bool Update { get; set; }

        [Obsolete("This property is obsolete", false)]
        public bool EditMode { get; set; }

        [Obsolete("The PagePermissions property is deprecated. Use PermissionList instead", false)]
        public string PagePermissions
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
    }

    public class PageTemplateModule
    {
        public PageTemplateModule()
        {
            ModuleDefinitionName = "";
            Title = "";
            Pane = PaneNames.Default;
            Order = 1;
            ContainerType = "";
            IsDeleted = false;
            PermissionList = new List<Permission>()
            {
                new Permission(PermissionNames.View, RoleNames.Admin, true),
                new Permission(PermissionNames.Edit, RoleNames.Admin, true)
            };
            Content = "";
        }

        public string ModuleDefinitionName { get; set; }
        public string Title { get; set; }
        public string Pane { get; set; }
        public int Order { get; set; }
        public string ContainerType { get; set; }
        public bool IsDeleted { get; set; }
        public List<Permission> PermissionList { get; set; }
        public string Content { get; set; }

        [Obsolete("The ModulePermissions property is deprecated. Use PermissionList instead", false)]
        public string ModulePermissions
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
    }
}

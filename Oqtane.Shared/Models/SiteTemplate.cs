using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Oqtane.Models
{
    public class SiteTemplate
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
    }

    public class PageTemplate
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public int Order { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public bool IsPersonalizable { get; set; }
        public List<Permission> PermissionList { get; set; }
        public List<PageTemplateModule> PageTemplateModules { get; set; }

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
        public string ModuleDefinitionName { get; set; }
        public string Title { get; set; }
        public string Pane { get; set; }
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

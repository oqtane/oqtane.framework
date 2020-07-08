using System;
using System.Collections.Generic;

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
        public string Path { get; set; }
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public bool IsPersonalizable { get; set; }
        public string PagePermissions { get; set; }
        public List<PageTemplateModule> PageTemplateModules { get; set; }

        [Obsolete("This property is obsolete", false)]
        public bool EditMode { get; set; }
    }

    public class PageTemplateModule
    {
        public string ModuleDefinitionName { get; set; }
        public string Title { get; set; }
        public string Pane { get; set; }
        public string ModulePermissions { get; set; }
        public string Content { get; set; }
    }
}

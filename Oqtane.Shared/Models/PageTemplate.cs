namespace Oqtane.Models
{
    public class PageTemplate
    {
        public string Name { get; set; }
        public string Parent { get; set; }
        public string Path { get; set; }
        public int Order { get; set; }
        public string Icon { get; set; }
        public bool IsNavigation { get; set; }
        public string PagePermissions { get; set; }
        public string ModuleDefinitionName { get; set; }
        public string ModulePermissions { get; set; }
        public string Title { get; set; }
        public string Pane { get; set; }
        public string ContainerType { get; set; }
    }
}

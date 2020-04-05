namespace Oqtane.Models
{
    public class Theme
    {
        public Theme()
        {
            Name = "";
            Version = "";
            Owner = "";
            Url = "";
            Contact = "";
            License = "";
            Dependencies = "";
        }

        public string ThemeName { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
        public string Url { get; set; }
        public string Contact { get; set; }
        public string License { get; set; }
        public string Dependencies { get; set; }
        public string ThemeControls { get; set; }
        public string PaneLayouts { get; set; }
        public string ContainerControls { get; set; }
        public string AssemblyName { get; set; }
    }
}

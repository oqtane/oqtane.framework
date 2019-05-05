namespace Oqtane.Models
{
    public class ModuleDefinition 
    {
        public string ModuleDefinitionName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Owner { get; set; }
        public string Url { get; set; }
        public string Contact { get; set; }
        public string License { get; set; }
        public string Dependencies { get; set; }
        public string ControlTypeTemplate { get; set; }
        public string ControlTypeRoutes { get; set; }
        public string AssemblyName { get; set; }
    }
}

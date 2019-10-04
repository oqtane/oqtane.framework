namespace Oqtane.Models
{
    public class Package
    {
        public string PackageId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public string Version { get; set; }
        public long Downloads { get; set; }
    }
}

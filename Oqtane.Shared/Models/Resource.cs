using Oqtane.Shared;

namespace Oqtane.Models
{
    public class Resource
    {
        public ResourceType ResourceType { get; set; }
        public string Url { get; set; }
        public string Integrity { get; set; }
        public string CrossOrigin { get; set; }
        public string Bundle { get; set; }
    }
}

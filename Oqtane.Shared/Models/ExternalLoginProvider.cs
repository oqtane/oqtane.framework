using System.Collections.Generic;

namespace Oqtane.Models
{
    public class ExternalLoginProvider
    {
        public string Name { get; set; }

        public Dictionary<string, string> Settings { get; set; }
    }
}

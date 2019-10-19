using System.Collections.Generic;

namespace Oqtane.Logging
{
    public class LogScope
    {
        public string Text { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
